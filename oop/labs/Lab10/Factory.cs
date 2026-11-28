namespace Lab10
{
    public class Factory : Organisation
    {
        public ProductTypeEnum ProductType { get; private set; }
        public int EngineerCount { get; private set; }

        public Factory() : base()
        {
            ProductType = ProductTypeEnum.NONE;
            EngineerCount = 0;
        }

        public Factory(string address, string name, int money,
            ProductTypeEnum productType, Worker[] workers, int engineers) : base(address, name, money, workers)
        {
            ProductType = productType;
            EngineerCount = engineers;
        }

        public Factory(Factory mold) : base(mold)
        {
            ProductType = mold.ProductType;
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
            EngineerCount = 0;
        }

        public override void RandomInit()
        {
            base.RandomInit();
            ProductType = (ProductTypeEnum)random.Next(0, 5);
            EngineerCount = random.Next(0, 20);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Factory other && base.Equals(other))
            {
                return other.ProductType == ProductType
                    && other.EngineerCount == EngineerCount;
            }
            return false;
        }

        public override object Clone()
        {
            return new Factory(this);
        }
    }

    public enum ProductTypeEnum { NONE, FOOD, CLOTHES, MACHINERY, CHEMICALS };
}
