using System;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.VisualBasic.CompilerServices;

namespace Assignment4WC.Models
{
    public class IntegerId
    {
        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                if (value < 0) throw new ArgumentException("Id cannot be less than 0");
                _value = value;
            }
        }

        private IntegerId(int id) => Value = id;

        public static implicit operator int(IntegerId id) => id.Value;
        public static explicit operator IntegerId(int id) => new(id);
    }
}

