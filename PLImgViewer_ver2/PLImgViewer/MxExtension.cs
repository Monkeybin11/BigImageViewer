using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeedyCoding;
using System.Runtime.InteropServices;

namespace PLImgViewer
{
	public static class MxExtension
	{
		public static void SaveRawData( this byte [ ] src , string path )
		{
			StringBuilder st = new StringBuilder();
			for ( int i = 0 ; i < src.GetLength( 0 ) ; i++ )
			{
				st.Append( src [ i ].ToString() );
				st.Append( "," );
			}
			System.IO.File.WriteAllText( path , st.ToString() );
		}

		public static void SaveRawData( this ushort [ ] src , string path )
		{
			StringBuilder st = new StringBuilder();
			for ( int i = 0 ; i < src.GetLength( 0 ) ; i++ )
			{
				st.Append( src [ i ].ToString() );
				st.Append( "," );
			}
			System.IO.File.WriteAllText( path , st.ToString() );
		}


		public static ushort [ ] TouShort( this byte [ ] src )
		{
			if ( src.GetLength( 0 ) % 2 != 0 ) Console.WriteLine( "Len is not even" );
			var len = src.GetLength(0)/2;


			List<ushort> output = new List<ushort>();
			for ( int i = 0 ; i < len ; i++ )
			{
				var target = new byte [ ] { src [ i * 2 ] , src [ i * 2 + 1]};
				var result =  BitConverter.ToUInt16( target , 0 );

				output.Add( result );
			}

			return output.ToArray();
		}
	}

	public static class IOExt
	{
		private static readonly uint[] _lookup32Unsafe = CreateLookup32Unsafe();

		private static unsafe readonly uint* _lookup32UnsafeP = (uint*)GCHandle.Alloc(_lookup32Unsafe,GCHandleType.Pinned).AddrOfPinnedObject();

		private static uint [ ] CreateLookup32Unsafe()
		{
			var result = new uint[256];
			for ( int i = 0 ; i < 256 ; i++ )
			{
				string s=i.ToString("X2");
				if ( BitConverter.IsLittleEndian )
					result [ i ] = ( ( uint )s [ 0 ] ) + ( ( uint )s [ 1 ] << 16 );
				else
					result [ i ] = ( ( uint )s [ 1 ] ) + ( ( uint )s [ 0 ] << 16 );
			}
			return result;
		}

		public static string ByteArrayToHexViaLookup32Unsafe( this byte [ ] bytes )
		{
			unsafe
			{
				var lookupP = _lookup32UnsafeP;
				var result = new char[bytes.Length * 2];
				fixed ( byte* bytesP = bytes )
				fixed ( char* resultP = result )
				{
					uint* resultP2 = (uint*)resultP;
					for ( int i = 0 ; i < bytes.Length ; i++ )
					{
						resultP2 [ i ] = lookupP [ bytesP [ i ] ];
					}
				}
				return new string( result );
			}
		}
	}

}
