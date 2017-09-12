using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SpeedyCoding;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace PLImgViewer
{
	/// <summary>
	/// Interaction logic for EmdFileExport.xaml
	/// </summary>
	public partial class EmdFileExport : Window
	{
		public EmdFileExport()
		{
			InitializeComponent();
		}

		public List<EmdInfoData> EmdData;
		public int TrackLimitation;
		public int BlockWidth;
		public int Width;
		public int Height;


		private void btnEmdLoad_Click( object sender , RoutedEventArgs e )
		{
			Stopwatch stw = new Stopwatch();

			OpenFileDialog ofd = new OpenFileDialog();
			string emdpath = "";

			if ( ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
			{
				emdpath = ofd.FileName;
			}
			else return;


			stw.Start();
			var charList = File.ReadAllBytes( emdpath )
						  .ByteArrayToHexViaLookup32Unsafe()
						  .ToCharArray();

			var grouped8bits = Enumerable.Range( 0 , charList.GetLength( 0 ) / 2 )
				.AsParallel().Select( x => new string (
								new char[] { charList [ x * 2 ] , charList [ x * 2 + 1 ] } ) )
				.ToArray<string>();

			var grouped16bits = Enumerable.Range(0,grouped8bits.GetLength(0)/2)
				.Select( x => grouped8bits [ x * 2 + 1 ] + grouped8bits [ x * 2 ])
				.ToArray<string>();

			var grouped32bits = Enumerable.Range(0,grouped16bits.GetLength(0)/2)
				.Select( x => grouped16bits [ x * 2 ] + grouped16bits [ x * 2 + 1 ])
				.ToArray<string>();

			Console.WriteLine( "Step1 : " + stw.ElapsedMilliseconds );
			stw.Restart();

			var indices = grouped32bits.IndicesOf( x => x == "FFFF0218") // Find Pattern
                                       .Select( x => x*4) // 32bits Len -> 8bits Len
                                       .ToArray<int>(); //  of 8bits 

			Console.WriteLine( "Step2 : " + stw.ElapsedMilliseconds );
			stw.Restart();
			Func<string[],int[],int> Hex4ToInt32 = // Local Function
                (lst , idx) =>
				{
					return Convert.ToInt32(
						 new string[]
						 {
							 "0x" ,
							 lst[idx[0]] ,
							 lst[idx[1]] ,
							 lst[idx[2]] ,
							 lst[idx[3]]
						 }
						 .Aggregate((f,s) => f + s) ,
								   16 ) ;
				};

			Console.WriteLine( "Step3 : " + stw.ElapsedMilliseconds );
			stw.Restart();

			var infos = indices.Select(x =>
							new {
								TrackNum = Hex4ToInt32(
									   grouped8bits ,
									   new int[] {x+15 ,x+14 ,x+13 ,x+12 })
								  ,
								BlockNum =  Hex4ToInt32(
									  grouped8bits ,
									  new int[] {x+7 ,x+6 ,x+5 ,x+4 })
								   ,
								DataCount =  Hex4ToInt32(
									  grouped8bits ,
									  new int[] {x+23 ,x+22 ,x+21 ,x+20 }) * 2 // This count is count of 16bits, need multiply 2(8bits) 
                                   ,
								Index = x
							}) // x is Header indices
                                    .ToList();
			Console.WriteLine( "Step4 : " + stw.ElapsedMilliseconds );
			stw.Restart();

			StringBuilder stb = new StringBuilder();
			infos.ActLoop( x =>
			 {
				 stb.Append( x.TrackNum.ToString() );
				 stb.Append( ',' );
				 stb.Append( x.BlockNum.ToString() );
				 stb.Append( ',' );
				 stb.Append( x.Index.ToString() );
				 stb.Append( Environment.NewLine );

			 }
			);
			var dir = System.IO.Path.GetDirectoryName( emdpath );

			File.WriteAllText( dir + "info.csv" , stb.ToString());

			Console.WriteLine( "Step5 : " + stw.ElapsedMilliseconds );
			stw.Restart();

			var swapped = infos.AsParallel().Select( x =>
								   grouped8bits
									.Skip(x.Index + 24)
									.Take(x.DataCount)
									.ToArray<string>())  // 8bits data Array [[Byte]]
                                .Select( x =>
								   Enumerable.Range(0,x.GetLength(0)/2)
                                      //.Select( s => Convert.ToInt32( x [ s * 2 + 1 ] + x [ s * 2 ] , 16)) //To 16bits
                                      .Select( s => (byte)(Convert.ToInt32( x [ s * 2 + 1 ] + x [ s * 2 ] , 16) * 255  / 65536.0) )  // To 8bits Normalize
                                      .ToArray<byte>())
								.ToList(); // swaped data , 16bits data Array [[Int32]]
			Console.WriteLine( "Step6 : " + stw.ElapsedMilliseconds );
			stw.Restart();

			var infoWithData =
				infos.Zip( swapped ,(f,s) => new EmdInfoData( f.TrackNum , f.BlockNum, s )
				).ToList(); // [( TrackNum , BlockNum , Data)]

			Console.WriteLine( "Step7 : " + stw.ElapsedMilliseconds );
			stw.Restart();

			var ordDataInfo = infoWithData
								.OrderBy( x => x.TrackNum )
								.ThenBy( x => x.BlockNum)
								.ToList(); // [( TrackNum , BlockNum , Data )] ordered by track and block number

			var trackLimit = infoWithData.Last().TrackNum;

			BlockWidth = infoWithData.Last().Data.Count();
			EmdData = ordDataInfo;
			TrackLimitation = trackLimit + 1;
			Width = BlockWidth * TrackLimitation;
		}

		private void btnEmdExtract_Click( object sender , RoutedEventArgs e )
		{
			FolderBrowserDialog ofd = new FolderBrowserDialog();
			ofd.SelectedPath = @"D:\03JobPro\2017\013_Polygon\Data";
			if ( ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
			{
				string dirpath = ofd.SelectedPath + "\\";
				for ( int i = 0 ; i < TrackLimitation ; i++ )
				{
					string path = dirpath + i.ToString("D3") +".dat";

					Stream outStream = new FileStream(path, FileMode.Create);


					var trackdata = EmdData.Where( x => x.TrackNum == i ).ToList();
					for ( int j = 0 ; j < trackdata.Count() ; j++ )
					{
						var datacount =  trackdata [ j ].Data.Count();
						outStream.Write( trackdata [ j ].Data , 0 , datacount );

					}
					outStream.Dispose();
					if ( i == 0 ) Height = trackdata.Count();
				}

				StringBuilder stb = new StringBuilder();
				//stb.Append( "TrackLimitation" );
				//stb.Append( "," );
				//stb.Append( "BlockWidth" );
				//stb.Append( "," );
				//stb.Append( "Width" );
				//stb.Append( "," );
				//stb.Append( "Height" );
				//stb.Append( Environment.NewLine );
				stb.Append( TrackLimitation );
				stb.Append( "," );
				stb.Append( BlockWidth );
				stb.Append( "," );
				stb.Append( Width );
				stb.Append( "," );
				stb.Append( Height );
				File.WriteAllText( dirpath + "info.txt" , stb.ToString() );
			}
		}

		private void btnToCsv_Click( object sender , RoutedEventArgs e )
		{
			FolderBrowserDialog ofd = new FolderBrowserDialog();
			ofd.SelectedPath = @"D:\03JobPro\2017\013_Polygon\Data";
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				string dirpath = ofd.SelectedPath + "\\";

				for ( int i = 0 ; i < TrackLimitation ; i++ )
				{
					string path = dirpath + i.ToString("D3") +".csv";
					StringBuilder stb = new StringBuilder();
					

					var trackdata = EmdData.Where( x => x.TrackNum == i ).ToList();
					for ( int j = 0 ; j < trackdata.Count() ; j++ )
					{
						trackdata[j].Data.ActLoop( a => {
							stb.Append( a.ToString() );
							stb.Append( ',' );
						} );
						stb.Append( Environment.NewLine );
					}
					if ( i == 0 ) Height = trackdata.Count();
					File.WriteAllText( path , stb.ToString() );
				}
			}
		}
	}
}
