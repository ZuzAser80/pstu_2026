namespace Lab10
{
    public class Worker
    {
        public string Name { get; private set; }
        public int Experience { get; private set; }
        public string Workshop { get; private set; }
        public string Profession { get; private set; }

        public Worker()
        {
            Name = "";
            Experience = 0;
            Workshop = "";
            Profession = "";
        }

        public Worker(string name, int experience, string workshop = "", string profession = "")
        {
            Name = name;
            Experience = experience;
            Workshop = workshop;
            Profession = profession;
        }

        public Worker(Worker mold)
        {
            Name = mold.Name;
            Experience = mold.Experience;
            Workshop = mold.Workshop;
            Profession = mold.Profession;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Worker other)
            {
                return false;
            }

            return Name == other.Name
                && Experience == other.Experience
                && Workshop == other.Workshop
                && Profession == other.Profession;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Experience, Workshop, Profession);
        }
    }
}
