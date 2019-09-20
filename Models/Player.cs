using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace homm_queue.Models
{
    public class Player
    {
        public string Name { get; set; }
        public bool CurrentTurn { get; set; }
    }
}
