using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using WMPLib;

namespace AudioFil
{
    class LogikaRadio
    {
        private SqlConnection con;
        private SqlCommand cmd;
        private SqlDataAdapter da;
        private DataTable dt;

        WindowsMediaPlayer player = new WindowsMediaPlayer();

        public async Task<SqlConnection> PolaczBaza()
        {
            con = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\huber\\Documents\\Visual Studio 2017 Projekty\\AudioFil\\AudioFil\\db.mdf\";Integrated Security=True;Connect Timeout=30");
            await con.OpenAsync();
            return con;
        }

        public DataTable GetRadia()
        {
            cmd = new SqlCommand("SELECT * FROM Stacje", con);
            da = new SqlDataAdapter(cmd);
            dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public async void DodajRadio(string nazwa, string url)
        {
            cmd = new SqlCommand("INSERT INTO Stacje('NazwaStacji', 'Url') VALUES(@param1, @param2)", con);

            cmd.Parameters.AddWithValue("@param1", nazwa);
            cmd.Parameters.AddWithValue("@param2", url);

            await cmd.ExecuteNonQueryAsync();
        }

        public void Graj(string adres)
        { 
            player.URL = adres;
            player.controls.play();
        }

        public void Stop()
        {
            player.controls.stop();
        }

        public string SprawdzStat()
        {
            string stat = player.status;

            return stat;
        }

        public void Zamknij()
        {
            cmd.Dispose();
            da.Dispose();
            con.Close();
        }
    }
}
