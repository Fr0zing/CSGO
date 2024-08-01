using System;

namespace CSGOCheatDetector.Models
{
    // Класс, представляющий подозрительный файл
    public class SuspiciousFile : IEquatable<SuspiciousFile>
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ModificationDate { get; set; }
        public DateTime AccessDate { get; set; }
        public string Extension { get; set; }
        public string FullPath { get; set; }

        // Метод для сравнения файлов по их полному пути
        public bool Equals(SuspiciousFile other)
        {
            if (other is null) return false;
            return FullPath == other.FullPath;
        }

        // Переопределение метода Equals для сравнения объектов
        public override bool Equals(object obj) => Equals(obj as SuspiciousFile);

        // Переопределение метода GetHashCode для корректной работы с хэш-таблицами
        public override int GetHashCode() => FullPath.GetHashCode();
    }
}
