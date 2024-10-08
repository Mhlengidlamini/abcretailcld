﻿namespace ABCRetail.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public double TotalPrice => Items.Sum(item => item.Price * item.Quantity);
    }
}
