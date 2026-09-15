namespace Lab10
{
    class Factory : Organisation
    {
        public ProductTypeEnum ProductType { get; private set; }
        public int WorkerCount { get; private set; }
        public int EngineerCount { get; private set; }

        public Factory() : base()
        {
            ProductType = ProductTypeEnum.NONE;
            WorkerCount = 0;
            EngineerCount = 0;
        }

        public Factory(string address, string name, int money,
            ProductTypeEnum productType, int workers, int engineers) : base(address, name, money)
        {
            ProductType = productType;
            WorkerCount = workers;
            EngineerCount = engineers;
        }

        public Factory(Factory mold) : base(mold)
        {
            ProductType = mold.ProductType;
            WorkerCount = mold.WorkerCount;
            EngineerCount = mold.EngineerCount;
        }

        public override void Show()
        {
            System.Console.WriteLine($"FACTORY: {Name} at {Address} earning {AnnualMoneyEarned}. \nProduct type: {ProductType}, {WorkerCount} workers, {EngineerCount} engineers");
        }

        public override void Init()
        {
            base.Init();
            ProductType = ProductTypeEnum.NONE;
            WorkerCount = 0;
            EngineerCount = 0;
        }

        public override void RandomInit()
        {
            base.RandomInit();
            ProductType = (ProductTypeEnum)random.Next(0, 5);
            WorkerCount = random.Next(1, 15);
            EngineerCount = random.Next(0, 20);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return ((Factory)obj).WorkerCount == WorkerCount
                && ((Factory)obj).ProductType == ProductType
                && ((Factory)obj).EngineerCount == EngineerCount;
            }
            return false;
        }
    }

    public enum ProductTypeEnum { NONE, FOOD, CLOTHES, MACHINERY, CHEMICALS };
}