﻿using BitMiracle.LibTiff.Classic;
using DeveMazeGenerator.Generators;
using DeveMazeGenerator.InnerMaps;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveMazeGenerator
{
    public partial class Maze
    {
        
        private ushort[][] GetColorMaps()
        {
            int colorMapSize = 256;
            int colorMapSizeMinusTwo = colorMapSize - 2;

            ushort[] colorMapRed = new ushort[colorMapSize];
            ushort[] colorMapGreen = new ushort[colorMapSize];
            ushort[] colorMapBlue = new ushort[colorMapSize];

            for (int i = 0; i < colorMapSize - 2; i++)
            {
                colorMapRed[i] = (ushort)(i * colorMapSize);
                colorMapGreen[i] = (ushort)((colorMapSizeMinusTwo - i) * colorMapSize);
                colorMapBlue[i] = 0;
            }

            colorMapRed[colorMapSize - 2] = 0;
            colorMapGreen[colorMapSize - 2] = 0;
            colorMapBlue[colorMapSize - 2] = 0;

            colorMapRed[colorMapSize - 1] = ushort.MaxValue;
            colorMapGreen[colorMapSize - 1] = ushort.MaxValue;
            colorMapBlue[colorMapSize - 1] = ushort.MaxValue;

            return new ushort[][] { colorMapRed, colorMapGreen, colorMapBlue };
        }



        private void SaveMazeAsImageDeluxeTiff(String fileName, List<MazePointPos> pathPosjes, Action<int, int> lineSavingProgress)
        {
            pathPosjes.Sort((first, second) =>
            {
                if (first.Y == second.Y)
                {
                    return first.X - second.X;
                }
                return first.Y - second.Y;
            });



            using (var tif = Tiff.Open(fileName, "w"))
            {
                tif.SetField(TiffTag.IMAGEWIDTH, this.Width - 1);
                tif.SetField(TiffTag.IMAGELENGTH, this.Height - 1);
                tif.SetField(TiffTag.BITSPERSAMPLE, 8);
                tif.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                tif.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                tif.SetField(TiffTag.ROWSPERSTRIP, 1);
                tif.SetField(TiffTag.COMPRESSION, Compression.LZW);


                int curpos = 0;

                byte[] color_ptr = new byte[(this.Width - 1) * 3];

                for (int y = 0; y < this.Height - 1; y++)
                {

                    for (int x = 0; x < this.Width - 1; x++)
                    {
                        byte r = 0;
                        byte g = 0;
                        byte b = 0;

                        MazePointPos curPathPos;
                        if (curpos < pathPosjes.Count)
                        {
                            curPathPos = pathPosjes[curpos];
                            if (curPathPos.X == x && curPathPos.Y == y)
                            {
                                r = curPathPos.RelativePos;
                                g = (byte)(255 - curPathPos.RelativePos);
                                b = 0;
                                curpos++;
                            }
                            else if (this.innerMap[x, y])
                            {
                                r = 255;
                                g = 255;
                                b = 255;
                            }
                        }
                        else if (this.innerMap[x, y])
                        {
                            r = 255;
                            g = 255;
                            b = 255;
                        }

                        color_ptr[x * 3 + 0] = r;
                        color_ptr[x * 3 + 1] = g;
                        color_ptr[x * 3 + 2] = b;
                    }
                    tif.WriteScanline(color_ptr, y);
                    lineSavingProgress(y, this.Height - 2);
                }

                tif.FlushData();
                tif.Close();
            }
        }


        private void SaveMazeAsImageDeluxeTiffWithChunks(String fileName, List<MazePointPos> pathPosjes, Action<int, int> lineSavingProgress)
        {
            const int tileSize = HybridInnerMap.GridSize;

            //Should actually be Width -1 -1 but since we use the full Width it's only once -1
            //This will count the amount of tiles per line so if it's 15 Pixels we still want 2 tiles of 8
            int tilesInWidth = (((this.Width - 1) / tileSize) + 1) * tileSize;
            int expectedPixelsWidthBasedOnTileSize = tilesInWidth * tileSize * tileSize;
            int expectedPixelsHeightBasedOnTileSize = (tileSize * tileSize);

            pathPosjes.Sort((first, second) =>
            {
                int firstXTile = first.X / tileSize;
                int firstYTile = first.Y / tileSize;

                int secondXTile = second.X / tileSize;
                int secondYTile = second.Y / tileSize;

                if (firstYTile != secondYTile)
                {
                    return firstYTile - secondYTile;
                }
                if (firstXTile != secondXTile)
                {
                    return firstXTile - secondXTile;
                }

                int firstXInTile = first.X % tileSize;
                int firstYInTile = first.Y % tileSize;

                int secondXInTile = second.X % tileSize;
                int secondYInTile = second.Y % tileSize;

                if (firstYInTile == secondYInTile)
                {
                    return firstXInTile - secondXInTile;
                }
                return firstYInTile - secondYInTile;
            });

            using (var tif = Tiff.Open(fileName, "w"))
            {
                tif.SetField(TiffTag.IMAGEWIDTH, this.Width - 1);
                tif.SetField(TiffTag.IMAGELENGTH, this.Height - 1);
                tif.SetField(TiffTag.BITSPERSAMPLE, 8);
                tif.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                tif.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                //tif.SetField(TiffTag.ROWSPERSTRIP, 1);
                tif.SetField(TiffTag.COMPRESSION, Compression.LZW);

                tif.SetField(TiffTag.TILEWIDTH, tileSize);
                tif.SetField(TiffTag.TILELENGTH, tileSize);

                int curpos = 0;

                byte[] color_ptr = new byte[tileSize * tileSize * 3];

                int tileNumber = 0;
                for (int startY = 0; startY < this.Height - 1; startY += tileSize)
                {

                    for (int startX = 0; startX < this.Width - 1; startX += tileSize)
                    {
                        int xMax = Math.Min(this.Width - 1 - startX, tileSize);
                        int yMax = Math.Min(this.Height - 1 - startY, tileSize);

                        for (int y = startY, othery = 0; othery < tileSize; y++, othery++)
                        {
                            for (int x = startX, otherx = 0; otherx < tileSize; x++, otherx++)
                            {
                                byte r = 0;
                                byte g = 0;
                                byte b = 0;
                                if (otherx >= xMax || othery >= yMax)
                                {
                                    //Not sure if needed but I'd like to ensure that any additional bytes
                                    //written to the image are 0.
                                }
                                else
                                {
                                    MazePointPos curPathPos;
                                    if (curpos < pathPosjes.Count)
                                    {
                                        curPathPos = pathPosjes[curpos];
                                        if (curPathPos.X == x && curPathPos.Y == y)
                                        {
                                            r = curPathPos.RelativePos;
                                            g = (byte)(255 - curPathPos.RelativePos);
                                            b = 0;
                                            curpos++;
                                        }
                                        else if (this.innerMap[x, y])
                                        {
                                            r = 255;
                                            g = 255;
                                            b = 255;
                                        }
                                    }
                                    else if (this.innerMap[x, y])
                                    {
                                        r = 255;
                                        g = 255;
                                        b = 255;
                                    }
                                }
                                int startPos = othery * tileSize * 3 + otherx * 3;

                                color_ptr[startPos + 0] = r;
                                color_ptr[startPos + 1] = g;
                                color_ptr[startPos + 2] = b;
                            }

                        }
                        
                        var result = tif.WriteEncodedTile(tileNumber, color_ptr, tileSize * tileSize * 3);
                        //var result = tif.WriteTile(color_ptr, startX / tileSize, startY / tileSize, 0, 0);
                        //var result = tif.WriteRawTile(tileNumber, color_ptr, tileSize * tileSize * 3);
                        //Result should not be -1

                        lineSavingProgress(tileNumber * tileSize * tileSize, this.Height - 2);

                        tileNumber++;
                    }


                }

                tif.FlushData();
                tif.Close();
            }
        }


        private void SaveMazeAsImageDeluxeTiffWithColorMap(String fileName, List<MazePointPos> pathPosjes, Action<int, int> lineSavingProgress)
        {
            pathPosjes.Sort((first, second) =>
            {
                if (first.Y == second.Y)
                {
                    return first.X - second.X;
                }
                return first.Y - second.Y;
            });



            using (var tif = Tiff.Open(fileName, "w"))
            {
                tif.SetField(TiffTag.IMAGEWIDTH, this.Width - 1);
                tif.SetField(TiffTag.IMAGELENGTH, this.Height - 1);
                tif.SetField(TiffTag.BITSPERSAMPLE, 8);
                tif.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                tif.SetField(TiffTag.PHOTOMETRIC, Photometric.PALETTE);
                tif.SetField(TiffTag.COLORMAP, GetColorMaps());
                tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                tif.SetField(TiffTag.ROWSPERSTRIP, 1);
                tif.SetField(TiffTag.COMPRESSION, Compression.LZW);

                int curpos = 0;

                byte[] color_ptr = new byte[this.Width - 1];

                for (int y = 0; y < this.Height - 1; y++)
                {

                    for (int x = 0; x < this.Width - 1; x++)
                    {
                        byte ding = 254;

                        MazePointPos curPathPos;
                        if (curpos < pathPosjes.Count)
                        {
                            curPathPos = pathPosjes[curpos];
                            if (curPathPos.X == x && curPathPos.Y == y)
                            {
                                float reinterpreted = ((float)curPathPos.RelativePos) / 255.0f * 253.0f;

                                ding = (byte)((byte)reinterpreted);
                                curpos++;
                            }
                            else if (this.innerMap[x, y])
                            {
                                ding = 255;
                            }
                        }
                        else if (this.innerMap[x, y])
                        {
                            ding = 255;
                        }

                        color_ptr[x] = ding;
                    }
                    tif.WriteScanline(color_ptr, y);
                    lineSavingProgress(y, this.Height - 2);
                }

                tif.FlushData();
                tif.Close();
            }
        }


        private void SaveMazeAsImageDeluxeWithDynamicallyGeneratedPathTiff(String fileName, IEnumerable<MazePointPos> pathPosjes, Action<int, int> lineSavingProgress)
        {

            //To be implemented
            return;




            ImageInfo imi = new ImageInfo(this.Width - 1, this.Height - 1, 8, false); // 8 bits per channel, no alpha 
            // open image for writing 
            PngWriter png = FileHelper.CreatePngWriter(fileName, imi, true);
            // add some optional metadata (chunks)
            png.GetMetadata().SetDpi(100.0);
            png.GetMetadata().SetTimeNow(0); // 0 seconds fron now = now
            png.CompLevel = 4;
            //png.GetMetadata().SetText(PngChunkTextVar.KEY_Title, "Just a text image");
            //PngChunk chunk = png.GetMetadata().SetText("my key", "my text .. bla bla");
            //chunk.Priority = true; // this chunk will be written as soon as possible




            for (int yChunkStart = 0; yChunkStart < this.Height - 1; yChunkStart += Maze.LineChunks)
            {
                var yChunkEnd = Math.Min(yChunkStart + Maze.LineChunks, this.Height - 1);

                var pathPointsHere = pathPosjes.Where(t => t.Y >= yChunkStart && t.Y < yChunkEnd).ToList();
                pathPointsHere.Sort((first, second) =>
                {
                    if (first.Y == second.Y)
                    {
                        return first.X - second.X;
                    }
                    return first.Y - second.Y;
                });
                int curpos = 0;

                for (int y = yChunkStart; y < yChunkEnd; y++)
                {
                    ImageLine iline = new ImageLine(imi);

                    for (int x = 0; x < this.Width - 1; x++)
                    {
                        int r = 0;
                        int g = 0;
                        int b = 0;

                        MazePointPos curPathPos;
                        if (curpos < pathPointsHere.Count)
                        {
                            curPathPos = pathPointsHere[curpos];
                            if (curPathPos.X == x && curPathPos.Y == y)
                            {
                                r = curPathPos.RelativePos;
                                g = 255 - curPathPos.RelativePos;
                                b = 0;
                                curpos++;
                            }
                            else if (this.innerMap[x, y])
                            {
                                r = 255;
                                g = 255;
                                b = 255;
                            }
                        }
                        else if (this.innerMap[x, y])
                        {
                            r = 255;
                            g = 255;
                            b = 255;
                        }

                        ImageLineHelper.SetPixel(iline, x, r, g, b);
                    }
                    png.WriteRow(iline, y);
                    lineSavingProgress(y, this.Height - 2);
                }
            }
            png.End();
        }
    }
}
