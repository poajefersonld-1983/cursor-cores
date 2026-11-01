using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

static class PointerStyles {
 public static readonly string[] Names={"Clássica","Moderna","Compacta","Alongada","Larga","Contorno","Chevron","Triângulo","Diamante","Mira","Círculo","Gota","Computer Use (original)"};
 static PointF[] Points(params float[] xy){var p=new PointF[xy.Length/2];for(int i=0;i<p.Length;i++)p[i]=new PointF(xy[i*2],xy[i*2+1]);return p;}
 public static Point Hotspot(int style,int size){if(style==12)return new Point((int)Math.Round(size*0.125),(int)Math.Round(size*0.125));return style>=8&&style<=10?new Point(size/2,size/2):new Point((int)Math.Round(size*0.125),(int)Math.Round(size*0.094));}
 public static Bitmap Render(Color color,int size,int style){
  style=Math.Max(0,Math.Min(Names.Length-1,style));
  using(var large=new Bitmap(size*8,size*8,PixelFormat.Format32bppArgb)){
   using(var g=Graphics.FromImage(large)){
    g.SmoothingMode=SmoothingMode.AntiAlias;g.ScaleTransform(size*8/32f,size*8/32f);
    float brightness=color.GetBrightness();Color border=brightness<0.32f?Color.White:Color.FromArgb(30,30,38);
    using(var b=new SolidBrush(color))using(var edge=new Pen(border,1.25f)){edge.LineJoin=LineJoin.Round;edge.StartCap=LineCap.Round;edge.EndCap=LineCap.Round;
     PointF[] p=null;
     switch(style){
      case 0:p=Points(4,3,4,26,10,20,15,29,19,27,14,18,23,18);break;
      case 1:p=Points(4,3,6,27,12,21,18,28,22,24,15,18,24,16);break;
      case 2:p=Points(4,3,4,23,10,18,14,25,18,23,14,16,22,16);break;
      case 3:p=Points(4,3,5,29,11,22,15,30,19,28,15,20,22,20);break;
      case 4:p=Points(4,3,4,28,13,21,21,29,25,25,17,18,29,17);break;
      case 5:p=Points(4,3,4,26,10,20,15,29,19,27,14,18,23,18);break;
      case 6:p=Points(4,3,7,28,14,19,27,15);break;
      case 7:p=Points(4,3,7,28,26,17);break;
      case 8:p=Points(16,3,29,16,16,29,3,16);break;
      case 9:
       using(var under=new Pen(border,4.5f))using(var fore=new Pen(color,2f)){
        foreach(var pen in new Pen[]{under,fore}){g.DrawLine(pen,16,3,16,11);g.DrawLine(pen,16,21,16,29);g.DrawLine(pen,3,16,11,16);g.DrawLine(pen,21,16,29,16);g.DrawEllipse(pen,11,11,10,10);}}
       g.FillEllipse(b,14.5f,14.5f,3,3);break;
      case 10:
       using(var outer=new Pen(border,5))using(var ring=new Pen(color,2.75f)){g.DrawEllipse(outer,5,5,22,22);g.DrawEllipse(ring,5,5,22,22);}g.FillEllipse(b,14,14,4,4);g.DrawEllipse(edge,14,14,4,4);break;
      case 11:
       using(var path=new GraphicsPath()){path.AddBezier(4,3,5,13,5,24,16,27);path.AddBezier(16,27,32,26,28,15,4,3);path.CloseFigure();g.FillPath(b,path);g.DrawPath(edge,path);}break;
      case 12:
       using(var path=ComputerUsePath())using(var outline=new Pen(Color.White,1.25f)){
        outline.LineJoin=LineJoin.Round;outline.StartCap=LineCap.Round;outline.EndCap=LineCap.Round;
        g.FillPath(b,path);g.DrawPath(outline,path);
       }break;
     }
     if(p!=null){if(style==5){using(var path=new GraphicsPath()){path.AddPolygon(p);using(var halo=new Pen(border,3.5f)){halo.LineJoin=LineJoin.Round;g.DrawPath(halo,path);}using(var outline=new Pen(color,1.75f)){outline.LineJoin=LineJoin.Round;g.DrawPath(outline,path);}}}else{g.FillPolygon(b,p);g.DrawPolygon(edge,p);}}
     if(style==8){using(var dot=new SolidBrush(border)){g.FillEllipse(dot,14.5f,14.5f,3,3);}}
    }
   }
   var result=new Bitmap(size,size,PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(result)){g.CompositingMode=CompositingMode.SourceCopy;g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=PixelOffsetMode.HighQuality;g.DrawImage(large,new Rectangle(0,0,size,size),0,0,large.Width,large.Height,GraphicsUnit.Pixel);}return result;
  }
 }
 // AgentCursor normalized Bezier coordinates, also verified as float constants
 // in the installed Windows Computer Use renderer. No screenshot tracing.
 static GraphicsPath ComputerUsePath(){
  var path=new GraphicsPath();
  Func<float,float,PointF> p=(x,y)=>new PointF(4+x*25,4+y*25);
  PointF start=p(0.00599f,0.15864f);
  path.AddBezier(start,p(-0.02364f,0.06456f),p(0.06169f,-0.02474f),p(0.15158f,0.00627f));
  path.AddLine(p(0.15158f,0.00627f),p(0.87634f,0.25652f));
  path.AddBezier(p(0.87634f,0.25652f),p(0.97594f,0.29096f),p(0.98340f,0.43547f),p(0.88794f,0.48095f));
  path.AddLine(p(0.88794f,0.48095f),p(0.59343f,0.62108f));
  path.AddLine(p(0.59343f,0.62108f),p(0.45955f,0.92925f));
  path.AddBezier(p(0.45955f,0.92925f),p(0.41611f,1.02925f),p(0.27801f,1.02146f),p(0.24510f,0.91717f));
  path.AddLine(p(0.24510f,0.91717f),start);path.CloseFigure();return path;
 }
 public static byte[] CursorFile(Color color,int size,int style){using(var bm=Render(color,size,style))using(var png=new MemoryStream()){bm.Save(png,ImageFormat.Png);byte[] bytes=png.ToArray();Point hot=Hotspot(style,size);using(var output=new MemoryStream())using(var w=new BinaryWriter(output)){w.Write((ushort)0);w.Write((ushort)2);w.Write((ushort)1);w.Write((byte)size);w.Write((byte)size);w.Write((ushort)0);w.Write((ushort)hot.X);w.Write((ushort)hot.Y);w.Write(bytes.Length);w.Write(22);w.Write(bytes);return output.ToArray();}}}
}
