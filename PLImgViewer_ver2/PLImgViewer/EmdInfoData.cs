using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImgViewer
{
	public class EmdInfoData
	{
		public int TrackNum;
		public int BlockNum;
		public byte[] Data;

		public EmdInfoData( int tracknum , int blocknum , byte [ ] data )
		{
			TrackNum = tracknum;
			BlockNum = blocknum;
			Data = data;
		}
	}
}
