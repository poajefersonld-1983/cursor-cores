// Implementação de persistência incluída na versão completa de novembro/2026.
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

partial class CursorCores {
 const string StartupKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
 const string StartupValue="CursorCores";
 static string ExecutablePath { get { return typeof(CursorCores).Assembly.Location; } }
 static string GuardSuffix { get { return WindowsIdentity.GetCurrent().User.Value; } }
 static string GuardMutexName { get { return @"Local\CursorCores.SizeGuard."+GuardSuffix; } }
 static string GuardStopName { get { return @"Local\CursorCores.StopGuard."+GuardSuffix; } }
 [StructLayout(LayoutKind.Sequential)] struct GuardPoint { public int x,y; }
 [StructLayout(LayoutKind.Sequential)] struct GuardCursorInfo { public int cbSize,flags;public IntPtr cursor;public GuardPoint point; }
 [StructLayout(LayoutKind.Sequential)] struct GuardIconInfo { public int icon;public uint x,y;public IntPtr mask,color; }
 [StructLayout(LayoutKind.Sequential)] struct GuardBitmap { public int type,width,height,widthBytes;public ushort planes,bitsPixel;public IntPtr bits; }
 [DllImport("user32.dll")] static extern bool GetCursorInfo(ref GuardCursorInfo info);
 [DllImport("user32.dll")] static extern IntPtr LoadCursor(IntPtr instance,uint id);
 [DllImport("user32.dll")] static extern bool GetIconInfo(IntPtr cursor,out GuardIconInfo info);
 [DllImport("gdi32.dll")] static extern int GetObject(IntPtr bitmap,int count,out GuardBitmap info);
 [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr value);
 static void SetGuardStartup(bool enable){
  using(var key=Registry.CurrentUser.CreateSubKey(StartupKey)){
   if(enable)key.SetValue(StartupValue,"\""+ExecutablePath+"\" --guard",RegistryValueKind.String);
   else key.DeleteValue(StartupValue,false);
  }
 }
 static void StartGuard(){
  SetGuardStartup(true);
  Process.Start(new ProcessStartInfo(ExecutablePath,"--guard"){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=Path.GetDirectoryName(ExecutablePath)});
 }
 static void StopGuard(){
  SetGuardStartup(false);
  try{using(var stop=EventWaitHandle.OpenExisting(GuardStopName))stop.Set();}catch(WaitHandleCannotBeOpenedException){}
 }
 static bool ReadGuardPreferences(out int size,out string directory){
  size=0;directory=null;string path=Path.Combine(data,"preferencias.txt");
  if(!File.Exists(path))return false;
  string[] lines=File.ReadAllLines(path);int argb,style;
  if(lines.Length<3||!int.TryParse(lines[0],out argb)||!int.TryParse(lines[1],out size)||!int.TryParse(lines[2],out style))return false;
  if(Array.IndexOf(SupportedSizes,size)<0||style<0||style>=PointerStyles.Names.Length)return false;
  directory=Path.Combine(data,argb.ToString("X8")+"-"+size+"-s"+style+"-v4");
  return true;
 }
 static bool HasGuardSize(IntPtr cursor,int size){
  GuardIconInfo info;if(cursor==IntPtr.Zero||!GetIconInfo(cursor,out info))return true;
  try{
   GuardBitmap bitmap;IntPtr handle=info.color!=IntPtr.Zero?info.color:info.mask;
   if(GetObject(handle,Marshal.SizeOf(typeof(GuardBitmap)),out bitmap)==0)return true;
   return bitmap.width==size&&bitmap.height==(info.color!=IntPtr.Zero?size:size*2);
  }finally{if(info.color!=IntPtr.Zero)DeleteObject(info.color);if(info.mask!=IntPtr.Zero)DeleteObject(info.mask);}
 }
 static bool GuardRefreshNeeded(int size,string directory){
  GuardCursorInfo current=new GuardCursorInfo();current.cbSize=Marshal.SizeOf(typeof(GuardCursorInfo));
  // Do not unhide or replace a cursor while Computer Use or typing has hidden it.
  if(!GetCursorInfo(ref current)||(current.flags&1)==0)return false;
  using(var key=Registry.CurrentUser.OpenSubKey(@"Control Panel\Cursors")){
   if(key==null||!String.Equals(key.GetValue("Arrow","") as string,Path.Combine(directory,files[0]),StringComparison.OrdinalIgnoreCase))return false;
   if(Convert.ToInt32(key.GetValue("CursorBaseSize",size))!=size)return true;
  }
  // Optional Windows roles (pen, pin, person) are not supported on every release.
  for(int i=0;i<15;i++)if(i!=6&&!HasGuardSize(LoadCursor(IntPtr.Zero,cursorIds[i]),size))return true;
  return false;
 }
 static void RunGuard(){
  bool owns=false;
  using(var mutex=new Mutex(false,GuardMutexName)){
   try{
    try{owns=mutex.WaitOne(0,false);}catch(AbandonedMutexException){owns=true;}
    if(!owns)return;
    using(var context=new GuardContext())Application.Run(context);
   }finally{if(owns)mutex.ReleaseMutex();}
  }
 }
 sealed class GuardContext : ApplicationContext {
  readonly NotifyIcon icon;
  readonly ContextMenuStrip menu;
  readonly System.Windows.Forms.Timer timer;
  readonly EventWaitHandle stop;
  DateTime lastAttempt=DateTime.MinValue;
  public GuardContext(){
   stop=new EventWaitHandle(false,EventResetMode.ManualReset,GuardStopName);stop.Reset();
   menu=new ContextMenuStrip();menu.Items.Add("Abrir Cursor Cores",null,(s,e)=>Process.Start(new ProcessStartInfo(ExecutablePath){UseShellExecute=true}));
   menu.Items.Add("Encerrar monitor e desativar início automático",null,(s,e)=>{SetGuardStartup(false);ExitThread();});
   icon=new NotifyIcon{Text="Cursor Cores · monitor de tamanho",Icon=SystemIcons.Application,ContextMenuStrip=menu,Visible=true};
   icon.DoubleClick+=(s,e)=>Process.Start(new ProcessStartInfo(ExecutablePath){UseShellExecute=true});
   timer=new System.Windows.Forms.Timer{Interval=1000};timer.Tick+=Tick;timer.Start();
  }
  void Tick(object sender,EventArgs args){
   if(stop.WaitOne(0)){ExitThread();return;}
   if((DateTime.UtcNow-lastAttempt).TotalSeconds<2)return;
   try{
    int size;string directory;if(!ReadGuardPreferences(out size,out directory)||!Directory.Exists(directory))return;
    if(!GuardRefreshNeeded(size,directory))return;
    lastAttempt=DateTime.UtcNow;
    using(var key=Registry.CurrentUser.CreateSubKey(@"Control Panel\Cursors"))key.SetValue("CursorBaseSize",size,RegistryValueKind.DWord);
    ApplySize(directory,size);
   }catch(Exception ex){
    lastAttempt=DateTime.UtcNow;
    try{File.WriteAllText(Path.Combine(data,"monitor-erro.txt"),DateTime.UtcNow.ToString("O")+"\r\n"+ex.Message);}catch{}
   }
  }
  protected override void ExitThreadCore(){timer.Stop();icon.Visible=false;timer.Dispose();icon.Dispose();menu.Dispose();stop.Dispose();base.ExitThreadCore();}
 }
 static void TestPersistence(){
  string previous=data;string temp=Path.Combine(Path.GetTempPath(),"CursorCores-Guard-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);
  try{
   data=temp;int count=0;
   foreach(int size in SupportedSizes){
    File.WriteAllLines(Path.Combine(data,"preferencias.txt"),new[]{Color.Gold.ToArgb().ToString(),size.ToString(),"12"});
    int actual;string directory;if(!ReadGuardPreferences(out actual,out directory)||actual!=size||!directory.EndsWith("-"+size+"-s12-v4"))throw new Exception("Falha nas preferências do monitor: "+size);
    count++;
   }
   File.WriteAllLines(Path.Combine(data,"preferencias.txt"),new[]{"0","31","0"});int invalid;string invalidDirectory;if(ReadGuardPreferences(out invalid,out invalidDirectory))throw new Exception("Tamanho inválido aceito pelo monitor.");
   File.WriteAllText(Path.Combine(root,"verificacao-persistencia.txt"),count+" tamanhos validados pelo leitor do monitor; inicialização e cursores ativos não alterados.");
  }finally{data=previous;Directory.Delete(temp,true);}
 }
}
