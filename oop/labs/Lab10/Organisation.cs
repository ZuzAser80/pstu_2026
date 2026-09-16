using System;
using System.Data.SqlTypes;
using LabsUtil;

namespace Lab10
{
    public class Organisation : IComparable, IInit, ICloneable
    {

        public string Address { get; private set; }
        private string _name = "";
        public string Name
        {
            get => _name;
            private set
            {
                if (value == "")
                {
                    System.Console.WriteLine($"Tried setting an empty name to: {this._name}");
                    return;
                }
                _name = value;
            }
        }
        public int AnnualMoneyEarned { get; private set; }

        protected Random random = new();

        #region Constructors    

        public Organisation()
        {
            Address = "";
            Name = "placeholder";
        }

        public Organisation(string address, string name, int moneyearned)
        {
            Address = address;
            Name = name;
            AnnualMoneyEarned = moneyearned;
        }

        public Organisation(Organisation mold)
        {
            Address = mold.Address;
            Name = mold.Name;
            AnnualMoneyEarned = mold.AnnualMoneyEarned;
        }

        #endregion

        public virtual void Show()
        {
            System.Console.WriteLine($"ORGANISATION: {Name}, located at: {Address}, earning: {AnnualMoneyEarned}");
        }

        public virtual void Init()
        {

            var name = PstuUtil.TryReadT<string>("Name: ");
            var address = PstuUtil.TryReadT<string>("Address: ");
            var money = PstuUtil.TryReadT<int>("Money earned annually:");
            Name = name;
            Address = address;
            AnnualMoneyEarned = money;
        }

        public virtual void RandomInit()
        {
            Name = System.Guid.NewGuid().ToString();
            Address = System.Guid.NewGuid().ToString();
            AnnualMoneyEarned = random.Next(100, 10000);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var other = (Organisation)obj;
            if (other.Name != Name || other.Address != Address || other.AnnualMoneyEarned != AnnualMoneyEarned)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object? obj)
        {
            if (obj is not Organisation other)
            {
                throw new ArgumentException("Object is not a Organisation.");
            }
            return AnnualMoneyEarned.CompareTo(other.AnnualMoneyEarned);
        }

        public Organisation ShallowCopy() //поверхностное копирование
        {
            return (Organisation)this.MemberwiseClone();
        }
        public virtual object Clone()
        {
            return new Organisation("Клон " + this.Name, this.Address, this.AnnualMoneyEarned);
        }
    }

    public class NameComparer : IComparer<Organisation>
    {
        public int Compare(Organisation? x, Organisation? y)
        {
            if (x is null || y is null)
                return 0;
            return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }
    }

    public class InitClass : IInit
    {
        public string Caption { get; private set; }
        public int Value { get; private set; }

        public InitClass()
        {
            Caption = "";
            Value = 0;
        }

        public void Init()
        {
            Caption = PstuUtil.TryReadT<string>("Caption: ");
            Value = PstuUtil.TryReadT<int>("Value: ");
        }

        public void RandomInit()
        {
            Caption = System.Guid.NewGuid().ToString();
            Value = new Random().Next(0, 1000);
        }

        public void Show()
        {
            System.Console.WriteLine($"INIT CLASS: {Caption}, value: {Value}");
        }
    }

}
