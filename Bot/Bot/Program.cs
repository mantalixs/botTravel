using Newtonsoft.Json;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Telegram.BotAPI.AvailableTypes;

namespace TelegramBot
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var client = new TelegramBotClient("7055153038:AAEAkzSDadwuU63FR9FwBfE4jhiKIlyuyS4");
            var offset = 0;

            while (true)
            {
                try
                {
                    var updates = await client.GetUpdatesAsync(offset);

                    if (updates.Any())
                    {
                        foreach (var update in updates)
                        {
                            if (update.Message != null && update.Message.Text != null)
                            {
                                var chatId = update.Message.Chat.Id;
                                var username = update.Message.Chat.Username ?? "Unknown";
                                Console.WriteLine($"Chat Id: {chatId}");
                                Console.WriteLine($"Username: {username}");
                                var dateTime = update.Message.Date;
                                Console.WriteLine(update.Message.Text);
                                Console.WriteLine($"Час: {dateTime}");
                                var keyword = update.Message.Text;
                                if (!string.IsNullOrEmpty(keyword))
                                {
                                    if (keyword.StartsWith("/addhotel"))
                                    {
                                        var parts = keyword.Split(' ', 4);
                                        if (parts.Length == 4)
                                        {
                                            var hotel = new
                                            {
                                                Hotel = parts[1],
                                                City = parts[2],
                                                LocationId = parts[3]
                                            };
                                            await AddHotel(client, chatId, hotel);
                                        }
                                        else
                                        {
                                            await client.SendMessageAsync(chatId, "/addhotel <назва_готелю> <місто> <locationId>");
                                        }
                                    }
                                    else if (keyword.StartsWith("/gethotel"))
                                    {
                                        var parts = keyword.Split(' ', 2);
                                        if (parts.Length == 2)
                                        {
                                            var hotelName = parts[1];
                                            await GetHotel(client, chatId, hotelName);
                                        }
                                        else
                                        {
                                            await client.SendMessageAsync(chatId, "/gethotel <назва_готелю>");
                                        }
                                    }
                                    else if (keyword.StartsWith("/deletehotel"))
                                    {
                                        var parts = keyword.Split(' ', 2);
                                        if (parts.Length == 2)
                                        {
                                            var hotelName = parts[1];
                                            await DeleteHotel(client, chatId, hotelName);
                                        }
                                        else
                                        {
                                            await client.SendMessageAsync(chatId, "/deletehotel <назва_готелю>");
                                        }
                                    }
                                    else if (keyword.StartsWith("/getlocation"))
                                    {
                                        var parts = keyword.Split(' ', 2);
                                        if (parts.Length == 2)
                                        {
                                            var locationId = parts[1];
                                            await GetLocationInfo(client, chatId, locationId);
                                        }
                                        else
                                        {
                                            await client.SendMessageAsync(chatId, "/getlocation <locationId>");
                                        }
                                    }
                                    else if (keyword.StartsWith("/getphotos"))
                                    {
                                        var parts = keyword.Split(' ', 2);
                                        if (parts.Length == 2)
                                        {
                                            var locationId = parts[1];
                                            await GetPhotos(client, chatId, locationId);
                                        }
                                        else
                                        {
                                            await client.SendMessageAsync(chatId, "/getphotos <locationId>");
                                        }
                                    }
                                    else if (keyword.StartsWith("/gethotelsity"))
                                    {
                                        var parts = keyword.Split(' ', 2);
                                        if (parts.Length == 4)
                                        {
                                            var searchQuery = parts[1].Trim();
                                            await GetHotelInfo(client, chatId, searchQuery);
                                        }
                                        else
                                        {
                                            await client.SendMessageAsync(chatId, "/gethotelsity <назва_міста>");
                                        }
                                    }


                                }

                                Console.WriteLine();
                            }

                            offset = update.UpdateId + 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка: {ex.Message}");
                }

                await Task.Delay(1000);
            }
        }

        private static async Task AddHotel(TelegramBotClient client, long chatId, object hotel)
        {
            using var httpClient = new HttpClient();
            var url = "https://localhost:7202/Hotel/add";
            var content = new StringContent(JsonConvert.SerializeObject(hotel), System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    await client.SendMessageAsync(chatId, "Готель успішно доданий.");
                }
                else
                {
                    await client.SendMessageAsync(chatId, $"Помилка: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                await client.SendMessageAsync(chatId, $"Помилка під час запиту до API: {ex.Message}");
            }
        }

        private static async Task GetHotel(TelegramBotClient client, long chatId, string hotelName)
        {
            using var httpClient = new HttpClient();
            var url = $"https://localhost:7202/Hotel/get?hotelName={hotelName}";

            try
            {
                var response = await httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    await client.SendMessageAsync(chatId, responseBody);
                }
                else
                {
                    await client.SendMessageAsync(chatId, $"Помилка: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                await client.SendMessageAsync(chatId, $"Помилка під час запиту до API: {ex.Message}");
            }
        }

        private static async Task DeleteHotel(TelegramBotClient client, long chatId, string hotelName)
        {
            using var httpClient = new HttpClient();
            var url = $"https://localhost:7202/Hotel/delete?hotelName={hotelName}";

            try
            {
                var response = await httpClient.DeleteAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    await client.SendMessageAsync(chatId, "Готель успішно видалено.");
                }
                else
                {
                    await client.SendMessageAsync(chatId, $"Помилка: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                await client.SendMessageAsync(chatId, $"Помилка під час запиту до API: {ex.Message}");
            }
        }

        private static async Task GetLocationInfo(TelegramBotClient client, long chatId, string locationId)
        {
            using var httpClient = new HttpClient();
            var apiKey = "3E2BCB222B0E4394B6E1E5EED02A0F8D";
            var url = $"https://api.content.tripadvisor.com/api/v1/location/{locationId}/details?key={apiKey}&language=en";

            try
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var locationData = JsonConvert.DeserializeObject<LocationDetails>(content);

                    if (locationData != null)
                    {
                        var locationDetails = $"Назва готелю: {locationData.Name}\n" +
                                              $"Широта: {locationData.Latitude}\n" +
                                              $"Довгота: {locationData.Longitude}\n" +
                                              $"Часовий пояс: {locationData.Timezone}\n" +
                                              $"Веб-сайт: {locationData.WebUrl}\n" +
                                              $"Опис: {locationData.Description}\n" +
                                              $"Рейтинг: {locationData.Rating}\n" +
                                              $"Кількість відгуків: {locationData.NumReviews}\n" +
                                              $"Ціновий рівень: {locationData.PriceLevel}\n" +
                                              $"Зручності: {string.Join(", ", locationData.Amenities ?? new List<string>())}\n" +
                                              $"Категорія: {locationData.Category?.LocalizedName}\n" +
                                              $"Стилі: {string.Join(", ", locationData.Styles ?? new List<string>())}\n";

                        if (locationData.Photo != null && locationData.Photo.Images?.Large?.Url != null)
                        {
                            var photoUrl = locationData.Photo.Images.Large.Url;

                            if (!photoUrl.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                            {
                                photoUrl = photoUrl.Replace(".png", ".jpg");
                            }

                            await client.SendPhotoAsync(chatId, photoUrl, caption: locationDetails);
                        }
                        else
                        {
                            await client.SendMessageAsync(chatId, locationDetails);
                        }
                    }
                    else
                    {
                        await client.SendMessageAsync(chatId, "Локацію не знайдено за вашим запитом.");
                    }
                }
                else
                {
                    await client.SendMessageAsync(chatId, $"Помилка: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                await client.SendMessageAsync(chatId, $"Помилка під час запиту до API: {ex.Message}");
            }
        }

        private static async Task GetPhotos(TelegramBotClient client, long chatId, string locationId)
        {
            using var httpClient = new HttpClient();
            var apiKey = "3E2BCB222B0E4394B6E1E5EED02A0F8D";
            var url = $"https://api.content.tripadvisor.com/api/v1/location/{locationId}/photos?key={apiKey}";

            try
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var photosData = JsonConvert.DeserializeObject<PhotosResponse>(content);

                    if (photosData?.Data != null && photosData.Data.Count > 0)
                    {
                        foreach (var photo in photosData.Data)
                        {
                            var photoUrl = photo.Images.Large.Url;
                            var caption = !string.IsNullOrEmpty(photo.Caption) ? photo.Caption : "Без підпису";
                            await client.SendPhotoAsync(chatId, photoUrl, caption: caption);
                        }
                    }
                    else
                    {
                        await client.SendMessageAsync(chatId, "Немає доступних фото для цієї локації.");
                    }
                }
                else
                {
                    await client.SendMessageAsync(chatId, $"Помилка: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                await client.SendMessageAsync(chatId, $"Помилка під час запиту до API: {ex.Message}");
            }
        }

        private static async Task GetHotelInfo(TelegramBotClient client, long chatId, string searchQuery)
        {
            using var httpClient = new HttpClient();
            var apiKey = "3E2BCB222B0E4394B6E1E5EED02A0F8D";
            var url = $"https://api.content.tripadvisor.com/api/v1/location/search?key={apiKey}&searchQuery={searchQuery}&language=en";

            try
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var hotelData = JsonConvert.DeserializeObject<Data>(content);

                    if (hotelData != null && hotelData.data != null && hotelData.data.Any())
                    {
                        var hotelInfoList = hotelData.data.Take(5);
                        foreach (var hotelInfo in hotelInfoList)
                        {
                            var hotelDetails = $"ID локації: {hotelInfo.location_id}\n" +
                                               $"Назва: {hotelInfo.name}\n" +
                                               $"Адреса 1: {hotelInfo.address_obj?.street1}\n" +
                                               $"Місто: {hotelInfo.address_obj?.city}\n" +
                                               $"Країна: {hotelInfo.address_obj?.country}\n" +
                                               $"Поштовий код: {hotelInfo.address_obj?.postalcode}\n" +
                                               $"Повна адреса: {hotelInfo.address_obj?.address_string}\n";

                            await client.SendMessageAsync(chatId, hotelDetails);
                        }
                    }
                    else
                    {
                        await client.SendMessageAsync(chatId, "Готелів не знайдено за вашим запитом.");
                    }
                }
                else
                {
                    await client.SendMessageAsync(chatId, $"Помилка: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                await client.SendMessageAsync(chatId, $"Помилка під час запиту до API: {ex.Message}");
            }
        }
    }
}



public class AddressObj
        {
            public string? street1 { get; set; }
            public string? city { get; set; }
            public string? country { get; set; }
            public string? postalcode { get; set; }
            public string? address_string { get; set; }
        }

        public class DatumSecond
        {
            public string? location_id { get; set; }
            public string? name { get; set; }
            public AddressObj? address_obj { get; set; }
        }

        public class Data
        {
            public List<DatumSecond>? data { get; set; }
        }

        public class LocationDetails
        {
            public string LocationId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string WebUrl { get; set; }
            public DetailedAddress AddressObj { get; set; }
            public List<Ancestor> Ancestors { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string Timezone { get; set; }
            public string WriteReview { get; set; }
            public RankingDetails RankingData { get; set; }
            public string Rating { get; set; }
            public string RatingImageUrl { get; set; }
            public int NumReviews { get; set; }
            public Dictionary<int, SubratingDetails> Subratings { get; set; }
            public int PhotoCount { get; set; }
            public string SeeAllPhotos { get; set; }
            public string PriceLevel { get; set; }
            public List<string> Amenities { get; set; }
            public string ParentBrand { get; set; }
            public string Brand { get; set; }
            public HotelCategory Category { get; set; }
            public List<HotelSubcategory> Subcategory { get; set; }
            public List<string> Styles { get; set; }
            public List<TripTypeDetails> TripTypes { get; set; }
            public Photo Photo { get; set; }
        }

        public class DetailedAddress
        {
            public string Street1 { get; set; }
            public string Street2 { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
            public string PostalCode { get; set; }
            public string AddressString { get; set; }
        }

        public class Ancestor
        {
            public string Level { get; set; }
            public string Name { get; set; }
            public string LocationId { get; set; }
        }


        public class RankingDetails
        {
            public string GeoLocationId { get; set; }
            public string RankingString { get; set; }
            public string GeoLocationName { get; set; }
            public string RankingOutOf { get; set; }
            public string Ranking { get; set; }
        }

        public class SubratingDetails
        {
            public string Name { get; set; }
            public string LocalizedName { get; set; }
            public string RatingImageUrl { get; set; }
            public string Value { get; set; }
        }

        public class HotelCategory
        {
            public string Name { get; set; }
            public string LocalizedName { get; set; }
        }

        public class HotelSubcategory
        {
            public string Name { get; set; }
            public string LocalizedName { get; set; }
        }

        public class TripTypeDetails
        {
            public string Name { get; set; }
            public string LocalizedName { get; set; }
            public int Value { get; set; }
        }

        public class Photo
        {
            public Images Images { get; set; }
        }

        public class Images
        {
            public Large Large { get; set; }
        }

        public class Large
        {
            public string Url { get; set; }
        }

        public class PhotosResponse
        {
            public List<Datum> Data { get; set; }
        }

        public class Datum
        {
            public long Id { get; set; }
            public bool IsBlessed { get; set; }
            public string Caption { get; set; }
            public DateTime PublishedDate { get; set; }
            public Images Images { get; set; }
            public string Album { get; set; }
            public Source Source { get; set; }
            public User User { get; set; }
        }

        public class Source
        {
            public string Name { get; set; }
            public string LocalizedName { get; set; }
        }

        public class User
        {
            public string? Username { get; set; }
        }
    

