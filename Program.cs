using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MapConvert;

internal static class Program
{
    private const int CHUNK_SIZE = 64;
    private static int w = 0;
    private static int h = 0;

    public static void Main(string[] args)
    {
        var img = Image.Load<Rgb24>("covid-19.bmp");
        w = img.Width / CHUNK_SIZE;
        h = img.Height / CHUNK_SIZE;
        var bytes = QuantizePalette(img);
        ImageToMap(bytes, "x.txt");

        Console.ReadKey();
    }

    private static void ImageToMap(byte[] image, string file_path)
    {
        var tilemap = "";
        foreach (var e in TILES.Reverse())
        {
            tilemap += "\"" + e.Value + "\",";
        }
        var tileamounts = "";
        var newlist = new List<string>();
        var i = 0;
        var tilemapint = 0;
        var tiles_string = "";
        foreach (var p in image.Reverse())
        {
            var tile = p.ToString();
            newlist.Add(p.ToString());
            if (newlist.Count > 1)
            {
                if (newlist[i - 1] != p.ToString())
                {
                    tileamounts += tilemapint.ToString() + ",";
                    tilemapint = 0;
                    if (i == 0)
                    {
                        tiles_string += "[" + tile;
                    }
                    if (i != 0 && i != 127)
                    {
                        tiles_string += "," + tile;
                    }
                    if (i == 127)
                    {
                        tiles_string += "," + tile + "],";
                    }
                }
                else if (i == 127)
                {
                    tiles_string += "],";
                }
            }
            if (newlist.Count == 1)
            {
                tileamounts += "[";
                tiles_string += "[" + tile;
            }
            i += 1;
            tilemapint += 1;
            if (i == 128)
            {
                i = 0;
                tileamounts += tilemapint.ToString() + "],";
                tilemapint = 0;
                newlist.Clear();
            }
        }
        tiles_string = "{\"saveVersion\":9,\"width\":" + w + ",\"height\":" + h +
            ",\"mapStats\":{\"name\":\"Generated\",\"month\":4,\"year\":0,\"worldTime\":9.920037269592286,\"deaths\":0,\"housesDestroyed\":0,\"population\":0},\"worldLaws\":{\"world_law_forever_peace\":{\"name\":\"world_law_forever_peace\",\"boolVal\":false,\"stringVal\":\"\"},\"world_law_peaceful_monsters\":{\"name\":\"world_law_peaceful_monsters\",\"boolVal\":false,\"stringVal\":\"\"},\"world_law_hunger\":{\"name\":\"world_law_hunger\",\"boolVal\":true,\"stringVal\":\"\"},\"world_law_grow_trees\":{\"name\":\"world_law_grow_trees\",\"boolVal\":true,\"stringVal\":\"\"},\"world_law_grow_grass\":{\"name\":\"world_law_grow_grass\",\"boolVal\":true,\"stringVal\":\"\"},\"world_law_kingdom_expansion\":{\"name\":\"world_law_kingdom_expansion\",\"boolVal\":true,\"stringVal\":\"\"},\"list\":[{\"name\":\"world_law_forever_peace\",\"boolVal\":false,\"stringVal\":\"\"},{\"name\":\"world_law_peaceful_monsters\",\"boolVal\":false,\"stringVal\":\"\"},{\"name\":\"world_law_hunger\",\"boolVal\":true,\"stringVal\":\"\"},{\"name\":\"world_law_grow_trees\",\"boolVal\":true,\"stringVal\":\"\"},{\"name\":\"world_law_grow_grass\",\"boolVal\":true,\"stringVal\":\"\"},{\"name\":\"world_law_kingdom_expansion\",\"boolVal\":true,\"stringVal\":\"\"}]},\"tileMap\":[" +
            string.Join(string.Empty, tilemap.Take(tilemap.Length - 1)) +
            "]," + "\"tileArray\":[" +
            string.Join(string.Empty, tiles_string.Take(tilemap.Length - 2)) +
            "]]," + "\"tileAmounts\":[" +
            string.Join(string.Empty, tileamounts.Take(tilemap.Length - 1)) +
            "]]," + "\"tiles\":[],\"cities\":[],\"actors\":[],\"buildings\":[],\"kingdoms\":[]}";

        //TODO: Zlib
        //尚未实现zlib, 因为算法未知
        File.WriteAllText(file_path, tiles_string);
    }

    private static byte[] QuantizePalette(Image<Rgb24> image)
    {
        var chunks_width = image.Width / CHUNK_SIZE;
        var chunks_height = image.Height / CHUNK_SIZE;
        var chunks_ratio = chunks_height / chunks_width;
        var newSize = new Size((int)(chunks_width * CHUNK_SIZE), (int)(Math.Ceiling(chunks_ratio * chunks_width * (decimal)CHUNK_SIZE)));
        image.Mutate(x => x
        .Resize(newSize)
        );
        return image.ToPalette();
    }

    //下面是Palette 算法, 由于我没有找到合适的  converted_image = image.im.convert("P", dither, palette.im) 算法, 所以这里写了一个, 自己看看合不合适

    private static byte[] ToPalette(this Image imagex)
    {
        using var ms = new MemoryStream();
        imagex.SaveAsBmp(ms);
        var image = new System.Drawing.Bitmap(ms);
        var result = new byte[image.Width * image.Height];
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = image.GetPixel(x, y);
                //Console.WriteLine(pixel);
                var closestColor = GetClosestColor(pixel, COLORS.ToArray());
                //Console.WriteLine(closestColor);
                var index = Array.IndexOf(COLORS.ToArray(), closestColor);
                result[y * image.Width + x] = (byte)index;
            }
        }
        return result;
    }

    private static int GetClosestColor(System.Drawing.Color pixel, int[] palette)
    {
        var minDistance = int.MaxValue;
        int closestColor = 0;
        foreach (var color in palette)
        {
            var distance = GetColorDistance(pixel, color);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestColor = color;
            }
        }
        return closestColor;
    }

    private static int GetColorDistance(System.Drawing.Color c1, int c2)
    {
        var rDiff = c1.R - c2;
        var gDiff = c1.G - c2;
        var bDiff = c1.B - c2;
        return rDiff * rDiff + gDiff * gDiff + bDiff * bDiff;
    }

    public static Dictionary<int, string> TILES = new(){
            {
                63,
                "mountains"},
            {
                62,
                "mountains"},
            {
                61,
                "mountains"},
            {
                60,
                "mountains"},
            {
                59,
                "mountains"},
            {
                58,
                "mountains"},
            {
                57,
                "mountains"},
            {
                56,
                "mountains"},
            {
                55,
                "mountains"},
            {
                54,
                "mountains"},
            {
                53,
                "mountains"},
            {
                52,
                "mountains"},
            {
                51,
                "mountains"},
            {
                50,
                "mountains"},
            {
                49,
                "mountains"},
            {
                48,
                "mountains"},
            {
                47,
                "mountains"},
            {
                46,
                "mountains"},
            {
                45,
                "mountains"},
            {
                44,
                "mountains"},
            {
                43,
                "mountains"},
            {
                42,
                "mountains"},
            {
                41,
                "mountains"},
            {
                40,
                "mountains"},
            {
                39,
                "mountains"},
            {
                38,
                "soil_high:wasteland_high"},
            {
                37,
                "soil_low:wasteland_low"},
            {
                36,
                "soil_high:swamp_high"},
            {
                35,
                "soil_low:swamp_low"},
            {
                34,
                "soil_high:jungle_high"},
            {
                33,
                "soil_low:jungle_low"},
            {
                32,
                "soil_high:infernal_high"},
            {
                31,
                "soil_low:infernal_low"},
            {
                30,
                "soil_high:mushroom_high"},
            {
                29,
                "soil_low:mushroom_low"},
            {
                28,
                "soil_high:enchanted_high"},
            {
                27,
                "soil_low:enchanted_low"},
            {
                26,
                "soil_high:savanna_high"},
            {
                25,
                "soil_low:savanna_low"},
            {
                24,
                "soil_high:corrupted_high"},
            {
                23,
                "soil_low:corrupted_low"},
            {
                22,
                "soil_high:field"},
            {
                21,
                "soil_low:road"},
            {
                20,
                "lava1"},
            {
                19,
                "soil_low:fireworks"},
            {
                18,
                "lava2"},
            {
                17,
                "soil_high:tnt"},
            {
                16,
                "lava3"},
            {
                15,
                "close_ocean"},
            {
                14,
                "deep_ocean"},
            {
                13,
                "soil_high:grass_high"},
            {
                12,
                "soil_high"},
            {
                11,
                "soil_high:snow_high"},
            {
                10,
                "hills"},
            {
                9,
                "hills:snow_hills"},
            {
                8,
                "mountains"},
            {
                7,
                "mountains:snow_block"},
            {
                6,
                "sand"},
            {
                5,
                "sand:snow_sand"},
            {
                4,
                "shallow_waters"},
            {
                3,
                "shallow_waters:ice"},
            {
                2,
                "soil_low"},
            {
                1,
                "soil_low:snow_low"},
            {
                0,
                "soil_low:grass_low"}};

    public static List<int> COLORS = new(){
            126,
            175,
            70,
            186,
            213,
            211,
            213,
            142,
            18,
            167,
            214,
            244,
            85,
            174,
            240,
            175,
            245,
            241,
            247,
            232,
            152,
            252,
            253,
            253,
            69,
            69,
            69,
            226,
            237,
            236,
            82,
            82,
            82,
            211,
            228,
            227,
            182,
            111,
            58,
            84,
            114,
            45,
            51,
            112,
            204,
            64,
            132,
            226,
            255,
            222,
            0,
            163,
            0,
            0,
            255,
            172,
            0,
            180,
            61,
            204,
            255,
            103,
            0,
            193,
            153,
            124,
            168,
            102,
            58,
            111,
            85,
            108,
            83,
            63,
            81,
            240,
            177,
            33,
            207,
            147,
            27,
            140,
            220,
            106,
            118,
            177,
            83,
            103,
            118,
            66,
            85,
            99,
            56,
            156,
            54,
            38,
            104,
            55,
            45,
            70,
            160,
            82,
            31,
            112,
            32,
            80,
            129,
            108,
            106,
            166,
            139,
            132,
            147,
            113,
            108,
            119,
            89,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253,
            252,
            253,
            253
        };
}