using SmellyEggPasswordManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SmellyEggPasswordManager.Controller
{
    public class ImageController : WebRequestBase
    {
        /// <summary>
        /// 获取每日最新的必应壁纸
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetOnlineImageUrlAsync()
        {
            var requestUrl = @"https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1";
            var result = await GetJsonObjectAsync(requestUrl);
            var wallpaperObject = Newtonsoft.Json.JsonConvert.DeserializeObject<MyBingModel>(result);
            var url = @"https://cn.bing.com" + wallpaperObject.images[0].url;
            //var resultImage = await LoadImage(new Uri(url));
            return url;
        }

        private async Task<BitmapImage> LoadImage(Uri uri)
        {
            BitmapImage bitmapImage = new BitmapImage();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (var response = await client.GetAsync(uri))
                    {
                        response.EnsureSuccessStatusCode();

                        using (var inputStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var memStream = new MemoryStream())
                            {
                                await inputStream.CopyToAsync(memStream);
                                bitmapImage.StreamSource = memStream;
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

                                //bitmapImage.SetSource(await inputStream.CopyToAsync(memStream));
                                //bitmapImage.GetValue(await inputStream.CopyToAsync(memStream));
                            }
                        }
                    }
                }
                return bitmapImage;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Failed to load the image: {0}", ex.Message);
            }

            return null;
        }
    }
}
