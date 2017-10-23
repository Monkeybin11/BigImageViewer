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
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;

namespace PLImgViewer
{
	
	public partial class EmdFileExport2
	{
		public List<EmdInfoData> EmdData;
		public int TrackLimitation;
		public int BlockWidth;

		double progress;
		double currentcount;
		object key = new object();


		public string Basepath = "";
		public bool IsRunning = false;
		public Task ProgressTask;
		public bool ProgressRun = true;

		private async void btnEmdLoad_Click( object sender , RoutedEventArgs e )
		{
			progress = 0.0;
			Stopwatch stwtotal = new Stopwatch();

			if ( !IsRunning )
			{
				try
				{


					IsRunning = true;
					OpenFileDialog ofd = new OpenFileDialog();
					string emdpath = "";

					if ( ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
					{
						stwtotal.Start();
						emdpath = ofd.FileName;
						Basepath = System.IO.Path.GetDirectoryName( emdpath );
						progress = 0.0;
						ProgressTask = Task.Run( () =>
						{

							
						} );
					}
					else
					{
						IsRunning = false;
						return;
					}


					Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

					Stopwatch stw = new Stopwatch();
					var task1 = Task.Factory.StartNew( () =>
					{

						stw.Start();
						var charList = File.ReadAllBytes( emdpath )
						  .ByteArrayToHexViaLookup32Unsafe()
						  .ToCharArray();

						var bt8list = File.ReadAllBytes( emdpath );

						progress = 1.0;
						var grouped8bits = Enumerable.Range( 0 , charList.GetLength( 0 ) / 2 )
							.AsParallel().AsOrdered().Select( x => new string (
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
						progress = 2.0;


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
							new
							{
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
						progress = 7.0;

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

						File.WriteAllText( dir +"\\"+ "info.csv" , stb.ToString() );

						Console.WriteLine( "Step5 : " + stw.ElapsedMilliseconds );
						stw.Restart();
						progress = 10.0;

						List<byte[]> swapped = null;
						double currentcount = 0;
						int totalcount = infos.Count();

						List<string[]>  stlist = infos.AsParallel().AsOrdered().Select( x =>
						{
							string[] res = null;
							try
							{
								res = grouped8bits
									.Skip( x.Index + 24 )
									.Take( x.DataCount )
									.ToArray<string>() ;
							}
							finally
							{
								lock(key)
								{
									currentcount = currentcount + 1;
									progress = 10.0 + 50*currentcount/totalcount;
								}
							}
							return  res;
						})

						.ToList();  // 8bits data Array [[Byte]]


						currentcount = 0;
						int totalcount2 = stlist.Count();
						swapped = stlist.AsParallel().AsOrdered().Select( x =>
						{
							byte[] res = new byte[] { };
							try
							{
								res = Enumerable.Range( 0 , x.GetLength( 0 ) / 2 )
										.Select( s => Convert.ToInt32( x [ s * 2 + 1 ] + x [ s * 2 ] , 16)) //To 16bits
										.Select( s => ( byte )( Convert.ToInt32( x [ s * 2 + 1 ] + x [ s * 2 ] , 16 ) * 255 / 65536.0 ) )  // To 8bits Normalize
										.ToArray<byte>();
							}
							finally
							{
								currentcount = currentcount + 1;
								progress = 60.0 + 10 * currentcount / totalcount2;
							}
							return res;
						} )
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
					} , TaskCreationOptions.LongRunning);

					await task1;
					Console.WriteLine( "task1 done " );

					Console.WriteLine( "Step8 : " + stw.ElapsedMilliseconds );
					stw.Restart();
					await ToImageList( Basepath , TrackLimitation , BlockWidth );
					progress = 99.0;
					Console.WriteLine( "Step9 : " + stw.ElapsedMilliseconds );
					stw.Restart();


					Mouse.OverrideCursor = null;
					IsRunning = false;
					progress = 100.0;



				}
				catch ( Exception )
				{
					System.Windows.MessageBox.Show( ".emd file is broken. please Check .emd File  " );
				}

				var totaltime = (stwtotal.ElapsedMilliseconds/1000).ToString();
				stwtotal.Stop();
				stwtotal.Reset();
			}

		}



		bool? rlreverssed = false;
		bool? topbotrev = false;

		Task<bool> ToImageList( string basepah , int tracknum , int w )
		{
			return Task<bool>.Run( () =>
			{
				try
				{
					var totalnum = EmdData.Count();
					byte[][] datalist = new byte[totalnum][];
					Bitmap[] bitmaps = new Bitmap[tracknum];

					Console.WriteLine( "Image start" );
					int counter = 0;

					Parallel.For( 0 , tracknum , i => {
						try
						{
							List<byte[]> trackdatas;
							if ( rlreverssed == true )
							{
								if ( i % 2 == 0 ) trackdatas = EmdData.Where( x => x.TrackNum == i )
																	  .Select( x => x.Data )
																	  .ToList();
								else trackdatas = EmdData.Where( x => x.TrackNum == i )
														 .Select( x => x.Data.Reverse().ToArray() )
														 .ToList();
							}
							else
							{
								trackdatas = EmdData.Where( x => x.TrackNum == i ).Select( x => x.Data ).ToList();
							}

							byte[] oneTrackdata = new byte[] { };

							if ( i % 2 == 0 ) oneTrackdata = trackdatas.Aggregate( ( f , s ) => f.Concat( s ).ToArray() );
							else
							{
								if ( topbotrev == true ) trackdatas.Reverse();
								oneTrackdata = trackdatas.Aggregate( ( f , s ) => f.Concat( s ).ToArray() );
							}
							bitmaps [ i ] = CopyDataToBitmap( oneTrackdata , w , trackdatas.Count() );



						}
						finally
						{
							counter++;
							progress = 70.0 + 29 * counter / tracknum;
						}


					} );

					for ( int i = 0 ; i < bitmaps.GetLength( 0 ) ; i++ )
					{
						bitmaps [ i ].Save( basepah + "\\" + i.ToString( "D3" ) + ".bmp" );
					}

					bitmaps.Select( x => new Image<Bgr , byte>( x ) )
						   .Aggregate( ( f , s ) => f.ConcateHorizontal( s ) )
						   .Save( basepah + "\\" + "Full.bmp" );
					return true;
				}
				catch ( Exception ex )
				{
					System.Windows.MessageBox.Show( "Data is too big to create full stitched image" );
					return false;
				}
			} );
		}

		public Bitmap CopyDataToBitmap( byte [ ] data , int w , int h )
		{
			//Here create the Bitmap to the know height, width and format
			Bitmap bmp = new Bitmap( w, h, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

			//Create a BitmapData and Lock all pixels to be written 
			BitmapData bmpData = bmp.LockBits(
					   new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
					   ImageLockMode.WriteOnly, bmp.PixelFormat);

			//Copy the data from the byte array into BitmapData.Scan0
			Marshal.Copy( data , 0 , bmpData.Scan0 , data.Length );


			//Unlock the pixels
			bmp.UnlockBits( bmpData );


			//Return the bitmap 
			return bmp;
		}


	}
}
