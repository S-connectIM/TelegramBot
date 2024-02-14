namespace aspmvc
{
    public class Utilities
    {
        private static string fileName = DateTime.Now.ToString().Replace("/", ".").Replace(":", ".");
        public static string logPath = $"telegram-bot\\Logs\\{fileName}.txt";
        public static string imagePath = $"telegram-bot\\Image\\";

        public static void EnsureDirectoriesExist()
        {
            string basePath = "./";
            logPath = Path.Combine(basePath, logPath);
            imagePath = Path.Combine(basePath, imagePath);
            Console.WriteLine(imagePath);
            string logDirectory = Path.GetDirectoryName(logPath);
            Console.WriteLine(Path.GetFullPath(logPath));
            if(!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
                Console.WriteLine(1);
            }

            if(!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
                Console.WriteLine(2);
            }
        }
    }
}
