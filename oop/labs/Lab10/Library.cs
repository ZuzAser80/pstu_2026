namespace Lab10
{
    class Library : Organisation
    {
        public BookGenresEnum BookGenres { get; private set; }
        public int BookCount { get; private set; }
        public int TextbookCount { get; private set; }

        public Library() : base()
        {
            BookGenres = BookGenresEnum.NONE;
            BookCount = 0;
            TextbookCount = 0;
        }

        public Library(string address, string name, int money,
            BookGenresEnum bookGenres, int books, int textbooks) : base(address, name, money)
        {
            BookGenres = bookGenres;
            BookCount = books;
            TextbookCount = textbooks;
        }

        public Library(Library mold) : base(mold)
        {
            BookGenres = mold.BookGenres;
            BookCount = mold.BookCount;
            TextbookCount = mold.TextbookCount;
        }

        public override void Show()
        {
            System.Console.WriteLine($"LIBRARY: {Name} at {Address} earning {AnnualMoneyEarned}. \nBook genres: {BookGenres}, holds {BookCount} books, {TextbookCount} textbooks");
        }

        public override void Init()
        {
            base.Init();
            BookGenres = BookGenresEnum.NONE;
            BookCount = 0;
            TextbookCount = 0;
        }

        public override void RandomInit()
        {
            base.RandomInit();
            BookGenres = (BookGenresEnum)random.Next(0, 5);
            BookCount = random.Next(1, 15);
            TextbookCount = random.Next(0, 500);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return ((Library)obj).BookCount == BookCount
                && ((Library)obj).BookGenres == BookGenres
                && ((Library)obj).TextbookCount == TextbookCount;
            }
            return false;
        }

        public override object Clone()
        {
            return new Library(this);
        }
    }

    public enum BookGenresEnum { NONE, FICTION, TEXTBOOK, JOURNAL, SCIENCE };
}