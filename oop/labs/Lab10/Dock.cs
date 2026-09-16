namespace Lab10;

class Dock : Organisation
{
    public ShipTypeEnum ShipType { get; private set; }
    public int ShipsInDock { get; private set; }
    public double ShipDisplacement { get; private set; }

    public Dock() : base()
    {
        ShipType = ShipTypeEnum.NONE;
        ShipsInDock = 0;
        ShipDisplacement = 0;
    }

    public Dock(string address, string name, int money, ShipTypeEnum shipType, int ships, double displacement) : base(address, name, money)
    {
        ShipType = shipType;
        ShipsInDock = ships;
        ShipDisplacement = displacement;
    }

    public Dock(Dock mold) : base(mold)
    {
        ShipType = mold.ShipType;
        ShipsInDock = mold.ShipsInDock;
        ShipDisplacement = mold.ShipDisplacement;
    }

    public override void Show()
    {
        System.Console.WriteLine($"DOCK: {Name} at {Address} earning {AnnualMoneyEarned}. \nShip type: {ShipType}, ships in dock: {ShipsInDock}, displacement: {ShipDisplacement} tons");
    }

    public override void Init()
    {
        base.Init();
        ShipType = ShipTypeEnum.NONE;
        ShipsInDock = 0;
        ShipDisplacement = 0;
    }

    public override void RandomInit()
    {
        base.RandomInit();
        ShipType = (ShipTypeEnum)random.Next(0, 6);
        ShipsInDock = random.Next(1, 15);
        ShipDisplacement = random.Next(1000, 50000);
    }

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return ((Dock)obj).ShipsInDock == ShipsInDock
            && ((Dock)obj).ShipType == ShipType
            && ((Dock)obj).ShipDisplacement == ShipDisplacement;
        }
        return false;
    }

    public override object Clone()
    {
        return new Dock(this);
    }
}

public enum ShipTypeEnum { NONE, CARGO, TANKER, LUXURY, MILITARY, SAILBOAT };