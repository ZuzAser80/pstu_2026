using System;
using System.Data.SqlTypes;
using LabsUtil;

namespace Lab10
{
    public class Organisation
    {
        
        public string Address { get; private set; }
        public string Name
        {
            get;
            private set
            {
                if (value == "")
                {
                    System.Console.WriteLine($"Tried setting an empty name to: {this.Name}");
                    return;
                }
                Name = value;
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
                        
            return base.Equals (obj);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

}
