using System.IO;
using BCnEncoder.Shared;
using BIS.Core.Streams;
using BIS.PAA;
using BIS.PAA.Encoder;
using Xunit;

namespace BIS.Core.Test
{
    public class PaaEncoderTest
    {
        [Fact]
        public void RectangularImageProducesMipmapsWithIndependentDimensions()
        {
            var image = new ColorRgba32[4, 8];
            for (var y = 0; y < image.GetLength(0); y++)
            {
                for (var x = 0; x < image.GetLength(1); x++)
                {
                    image[y, x] = new ColorRgba32((byte)(x * 20), (byte)(y * 40), 80, 255);
                }
            }

            using var output = new MemoryStream();
            using (var writer = new BinaryWriterEx(output, true))
            {
                PaaEncoder.WritePAA(writer, image, PAAType.DXT1);
            }

            output.Position = 42;
            using var reader = new BinaryReader(output, System.Text.Encoding.ASCII, true);
            Assert.Equal(64, reader.ReadInt32());
            var firstOffset = reader.ReadUInt32();
            var secondOffset = reader.ReadUInt32();

            output.Position = firstOffset;
            Assert.Equal((ushort)8, reader.ReadUInt16());
            Assert.Equal((ushort)4, reader.ReadUInt16());

            output.Position = secondOffset;
            Assert.Equal((ushort)4, reader.ReadUInt16());
            Assert.Equal((ushort)2, reader.ReadUInt16());
        }
    }
}
