﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IHelperRepository
    {
        public Task SaveJson(string jsonText , string filename);

        public Task SaveXmlResponse(string filename, string response);

        public Task<string> generatePassword();
    }
}
