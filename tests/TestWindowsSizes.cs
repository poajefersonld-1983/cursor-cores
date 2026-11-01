using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
class TestWindowsSizes {
 static int checks=0,images=0;
 static readonly BindingFlags S=BindingFlags.NonPublic|BindingFlags.Static,I=BindingFlags.NonPublic|BindingFlags.Instance;
 static int[] sizes={24,32,40,48,56,64};
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern IntPtr LoadImage(IntPtr h,string p,uint t,int x,int y,uint flags);
 [DllImport("user32.dll")] static extern bool DestroyCursor(IntPtr h);
 static void Assert(bool value,string message){checks++;if(!value)throw new Exception(message);}
 static void CheckCur(byte[] b,int size){
  Assert(BitConverter.ToUInt16(b,2)==2,"Tipo CUR");Assert(b[6]==size&&b[7]==size,"Dimensão CUR");
  Assert(BitConverter.ToUInt16(b,10)<size&&BitConverter.ToUInt16(b,12)<size,"Hotspot");
  using(var m=new MemoryStream(b,BitConverter.ToInt32(b,18),BitConverter.ToInt32(b,14)))using(var im=Image.FromStream(m))Assert(im.Width==size&&im.Height==size,"Dimensão PNG");images++;
 }
 static void CheckAni(byte[] b,int from,int end,int size){for(int p=from;p+8<=end;){int n=BitConverter.ToInt32(b,p+4);string id=System.Text.Encoding.ASCII.GetString(b,p,4);Assert(p+8+n<=end,"Chunk ANI");if(id=="icon"){byte[] c=new byte[n];Array.Copy(b,p+8,c,0,n);CheckCur(c,size);}else if(id=="LIST")CheckAni(b,p+12,p+8+n,size);p+=8+n+(n%2);}}
 [STAThread] static int Main(){
  string temp=Path.Combine(Path.GetTempPath(),"CursorSizes-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);
  bool native=Environment.OSVersion.Platform==PlatformID.Win32NT;
  try{
   Application.EnableVisualStyles();typeof(CursorCores).GetField("data",S).SetValue(null,temp);
   Assert(String.Join(",",(int[])typeof(CursorCores).GetField("SupportedSizes",S).GetValue(null))=="24,32,40,48,56,64","Sequência de tamanhos");
   using(var form=(Form)Activator.CreateInstance(typeof(CursorCores),true)){
    var combo=(ComboBox)typeof(CursorCores).GetField("sizes",I).GetValue(form);Assert(combo.Items.Count==6,"Seis opções");
    Assert((int)typeof(CursorCores).GetProperty("SelectedSize",I).GetValue(form,null)==32,"Padrão 32 px");
    for(int i=0;i<sizes.Length;i++){combo.SelectedIndex=i;Assert((int)typeof(CursorCores).GetProperty("SelectedSize",I).GetValue(form,null)==sizes[i],"Seleção exata");}
    combo.SelectedIndex=1;((ComboBox)typeof(CursorCores).GetField("styles",I).GetValue(form)).SelectedIndex=12;typeof(CursorCores).GetField("chosen",I).SetValue(form,Color.Gold);
    form.Show();Application.DoEvents();using(var shot=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(shot,new Rectangle(Point.Empty,form.Size));shot.Save("docs/imagens/interface-windows-teste.png",ImageFormat.Png);}form.Close();
   }
   foreach(int size in sizes){File.WriteAllLines(Path.Combine(temp,"preferencias.txt"),new[]{Color.Gold.ToArgb().ToString(),size.ToString(),"12"});using(var f=(Form)Activator.CreateInstance(typeof(CursorCores),true))Assert((int)typeof(CursorCores).GetProperty("SelectedSize",I).GetValue(f,null)==size,"Preferência "+size);}
   File.Delete(Path.Combine(temp,"preferencias.txt"));
   foreach(int size in sizes)for(int style=0;style<13;style++)foreach(Color color in new[]{Color.Gold,Color.Black,Color.FromArgb(155,89,255)})CheckCur(PointerStyles.CursorFile(color,size,style),size);
   foreach(int size in sizes)foreach(string name in (string[])typeof(CursorCores).GetField("files",S).GetValue(null)){
    byte[] b=(byte[])typeof(CursorCores).GetMethod("ConvertFile",S).Invoke(null,new object[]{name,Color.Gold,size});if(name.EndsWith(".cur"))CheckCur(b,size);else CheckAni(b,12,b.Length,size);
    if(native){string path=Path.Combine(temp,size+"-"+name);File.WriteAllBytes(path,b);IntPtr h=LoadImage(IntPtr.Zero,path,2,size,size,0x10);Assert(h!=IntPtr.Zero,"LoadImage Windows "+name+" "+size);DestroyCursor(h);}
   }
   using(var sheet=new Bitmap(620,170))using(var g=Graphics.FromImage(sheet))using(var font=new Font("Arial",12)){g.Clear(Color.FromArgb(235,238,244));for(int i=0;i<sizes.Length;i++){using(var im=PointerStyles.Render(Color.Gold,sizes[i],12))g.DrawImageUnscaled(im,22+i*100,35);g.DrawString(sizes[i]+" px",font,Brushes.Black,18+i*100,125);}sheet.Save("docs/imagens/comparacao-tamanhos.png",ImageFormat.Png);}
   Console.WriteLine("OK: "+checks+" verificações, "+images+" imagens/quadros; seis tamanhos e preferências antigas validados.");
   Console.WriteLine(native?"Carregador nativo Windows validado.":"Mono: interface e renderização validadas; carregador nativo Windows não executado.");return 0;
  }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}finally{Directory.Delete(temp,true);}
 }
}
