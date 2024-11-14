using Socket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Socket.Services.SingleTon
{
    public class UserDataManager
    {
        private static UserDataManager instance;
        private static readonly object lockObject = new object();
        private List<User> users;

        private UserDataManager()
        {
            LoadUsers();
            PrintAllUsers();
        }

        public static UserDataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new UserDataManager();
                        }
                    }
                }
                return instance;
            }
        }

        private void LoadUsers()
        {
            string json = File.ReadAllText("Data/users.json");
            users = JsonSerializer.Deserialize<List<User>>(json);
        }

        public void PrintAllUsers()
        {
            try
            {
                string json = File.ReadAllText("Data/users.json");
                List<User> users = JsonSerializer.Deserialize<List<User>>(json);

                Console.WriteLine("Danh sách tài khoản:");
                Console.WriteLine("--------------------");

                foreach (var user in users)
                {
                    Console.WriteLine($"Username: {user.username}");
                    Console.WriteLine($"Password: {user.password}");
                    Console.WriteLine("--------------------");
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Không tìm thấy file users.json");
            }
            catch (JsonException)
            {
                Console.WriteLine("Lỗi định dạng JSON");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
            }
        }

        public bool ValidateUser(string username, string password)
        {
            return users.Exists(u => u.username == username && u.password == password);
        }
    }
}
