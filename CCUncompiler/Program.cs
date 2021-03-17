using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using ValveResourceFormat.ClosedCaptions;

namespace CCUncompiler
{
    class Program
    {
        static void Main(string[] args)
        {

            var CCOb = new ClosedCaptions();
            //read original local file
            CCOb.Read("E:/Blender A new Generation/Scripts/alyx/subtitles/closecaption_english.bak.dat");



            /*
            uint strHask = ValveResourceFormat.Crc32.Compute(System.Text.Encoding.Unicode.GetBytes("<clr:18,199,226>Alyx confirmed."));
            Console.WriteLine(strHask);
            foreach (var item in CCOb.Captions)
            {
                if (item.Hash == strHask) Console.WriteLine("Its the HASH");
                if (item.UnknownV2 == strHask) Console.WriteLine("Its the Unkown" + strHask);
            }
            
            */

            //adding own caption
            ushort offset = 0;
            foreach (var item in CCOb.Captions.FindAll((e) => { return e.Blocknum == 130; }))
            {
                offset += item.Length;
            }
            
            var cap = new ClosedCaption
            {
                Hash = ValveResourceFormat.Crc32.Compute(System.Text.Encoding.Unicode.GetBytes("vo.OwnVoice")),
                Blocknum = 130,
                Offset = offset,
                Length = (ushort)System.Text.Encoding.Unicode.GetBytes("<clr:19,199,226>MyOwnLine").Length,
                UnknownV2 = ValveResourceFormat.Crc32.Compute(System.Text.Encoding.Unicode.GetBytes("<clr:19,199,226>MyOwnLine"))

            };
            //ClosedCaptions.directorysize+=1;
            cap.Text = "<clr:19,199,226>MyOwnLine";
            //CCOb.Captions.Add(cap);


            //replaces text with Testing
            int currblocknum = 0;
            int curroffset = 0;
            foreach (var item in CCOb.Captions)
            {
                
                item.Text = "Testing";
                item.Length = (ushort)System.Text.Encoding.Unicode.GetBytes(item.Text).Length;
                item.UnknownV2 = ValveResourceFormat.Crc32.Compute(System.Text.Encoding.Unicode.GetBytes(item.Text));
                item.Offset = (ushort)(curroffset);
                if(item.Offset+ item.Length >= ClosedCaptions.blocksize)
                {
                    item.Offset = 0;
                    currblocknum++;
                    curroffset = 0;
                }
                item.Blocknum = currblocknum;
                curroffset += item.Length;
            }
            ClosedCaptions.numblocks = (uint)currblocknum;
            Console.WriteLine(ClosedCaptions.blocksize);
            Console.WriteLine(ClosedCaptions.dataoffset);
            Console.WriteLine(ClosedCaptions.directorysize);
            Console.WriteLine(ClosedCaptions.numblocks);
            Console.WriteLine(ClosedCaptions.version);
            Console.WriteLine(CCOb.Captions.Count);
            //Write to file
            write("closecaption_english.dat", CCOb.Captions, File.OpenWrite("E:/Blender A new Generation/Scripts/alyx/subtitles/closecaption_english.dat"));
        }
        public static void write(string filename, List<ClosedCaption> Captions, Stream input)
        {
            var writer = new BinaryWriter(input, UTF8Encoding.UTF8);
            writer.Write(ClosedCaptions.MAGIC);
            writer.Write(ClosedCaptions.version);

            // numblocks, not actually required for hash lookups or populating entire list
            writer.Write(ClosedCaptions.numblocks);

            writer.Write(ClosedCaptions.blocksize);
            writer.Write(ClosedCaptions.directorysize);
            writer.Write(ClosedCaptions.dataoffset);
            for (int i = 0; i < Captions.Count; i++)
            {
                var caption = Captions[i];

                writer.Write(caption.Hash);
                if (ClosedCaptions.version >= 2)
                {
                    writer.Write(caption.UnknownV2);
                }

                writer.Write(caption.Blocknum);
                writer.Write(caption.Offset);
                writer.Write(caption.Length);

            }

            // Probably could be inside the for loop above, but I'm unsure what the performance costs are of moving the position head manually a bunch compared to reading sequentually
            for (int i = 0; i < Captions.Count; i++)
            {
                var caption = Captions[i];
                writer.BaseStream.Position = ClosedCaptions.dataoffset + (caption.Blocknum * ClosedCaptions.blocksize) + caption.Offset;
                writer.Write(System.Text.Encoding.Unicode.GetBytes(caption.Text));
                //Console.WriteLine(caption.Text+" position= "+ writer.BaseStream.Position);
            }
            writer.BaseStream.Position = ClosedCaptions.dataoffset + (ClosedCaptions.numblocks * ClosedCaptions.blocksize)-(ClosedCaptions.blocksize / 256)+ 31;
            writer.Write(new byte());
            writer.Flush();
            writer.Close();


        }

    }
}
