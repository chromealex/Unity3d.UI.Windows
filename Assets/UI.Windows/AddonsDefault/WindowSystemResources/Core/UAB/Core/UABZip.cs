using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;

namespace ME.UAB {

	[System.Serializable]
	public static class Zipper {

		public static byte[] ZipBytes(byte[] sBuffer) {
			MemoryStream m_msBZip2 = null;
			BZip2OutputStream m_osBZip2 = null;
			byte[] result;
			try {
				m_msBZip2 = new MemoryStream();
				Int32 size = sBuffer.Length;
				// Prepend the compressed data with the length of the uncompressed data (firs 4 bytes)
				//
				using (BinaryWriter writer = new BinaryWriter(m_msBZip2, System.Text.Encoding.ASCII)) {
					writer.Write(size);

					m_osBZip2 = new BZip2OutputStream(m_msBZip2);
					m_osBZip2.Write(sBuffer, 0, sBuffer.Length);

					m_osBZip2.Close();
					result = m_msBZip2.ToArray();
					m_msBZip2.Close();

					writer.Close();
				}
			} finally {
				if (m_osBZip2 != null) {
					m_osBZip2.Dispose();
				}
				if (m_msBZip2 != null) {
					m_msBZip2.Dispose();
				}
			}
			return result;
		}

		public static byte[] ZipString(string sBuffer) {
			MemoryStream m_msBZip2 = null;
			BZip2OutputStream m_osBZip2 = null;
			byte[] result;
			try {
				m_msBZip2 = new MemoryStream();
				Int32 size = sBuffer.Length;
				// Prepend the compressed data with the length of the uncompressed data (firs 4 bytes)
				//
				using (BinaryWriter writer = new BinaryWriter(m_msBZip2, System.Text.Encoding.ASCII)) {
					writer.Write(size);

					m_osBZip2 = new BZip2OutputStream(m_msBZip2);
					m_osBZip2.Write(Encoding.ASCII.GetBytes(sBuffer), 0, sBuffer.Length);

					m_osBZip2.Close();
					result = m_msBZip2.ToArray();
					m_msBZip2.Close();

					writer.Close();
				}
			} finally {
				if (m_osBZip2 != null) {
					m_osBZip2.Dispose();
				}
				if (m_msBZip2 != null) {
					m_msBZip2.Dispose();
				}
			}
			return result;
		}

		public static byte[] UnzipBytes(byte[] compbytes) {
			byte[] result;
			MemoryStream m_msBZip2 = null;
			BZip2InputStream m_isBZip2 = null;
			try {
				m_msBZip2 = new MemoryStream(compbytes);
				// read final uncompressed string size stored in first 4 bytes
				//
				using (BinaryReader reader = new BinaryReader(m_msBZip2, System.Text.Encoding.ASCII)) {
					Int32 size = reader.ReadInt32();

					m_isBZip2 = new BZip2InputStream(m_msBZip2);
					byte[] bytesUncompressed = new byte[size];
					m_isBZip2.Read(bytesUncompressed, 0, bytesUncompressed.Length);
					m_isBZip2.Close();
					m_msBZip2.Close();

					result = bytesUncompressed;

					reader.Close();
				}
			} finally {
				if (m_isBZip2 != null) {
					m_isBZip2.Dispose();
				}
				if (m_msBZip2 != null) {
					m_msBZip2.Dispose();
				}
			}
			return result;
		}

		public static string UnzipString(byte[] compbytes) {
			string result;
			MemoryStream m_msBZip2 = null;
			BZip2InputStream m_isBZip2 = null;
			try {
				m_msBZip2 = new MemoryStream(compbytes);
				// read final uncompressed string size stored in first 4 bytes
				//
				using (BinaryReader reader = new BinaryReader(m_msBZip2, System.Text.Encoding.ASCII)) {
					Int32 size = reader.ReadInt32();

					m_isBZip2 = new BZip2InputStream(m_msBZip2);
					byte[] bytesUncompressed = new byte[size];
					m_isBZip2.Read(bytesUncompressed, 0, bytesUncompressed.Length);
					m_isBZip2.Close();
					m_msBZip2.Close();

					result = Encoding.ASCII.GetString(bytesUncompressed);

					reader.Close();
				}
			} finally {
				if (m_isBZip2 != null) {
					m_isBZip2.Dispose();
				}
				if (m_msBZip2 != null) {
					m_msBZip2.Dispose();
				}
			}
			return result;
		}

	}

}
