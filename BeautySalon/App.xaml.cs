using BeautySalon.Model;
using System.Configuration;
using System.Data;
using System.Windows;

namespace BeautySalon
{
    public partial class App : Application
    {
        public static Model.BeautySalonContext Context{ get; set; } = new Model.BeautySalonContext();
        public static Model.User CurrentUser { get; set; } = null!;
    }

}
