﻿using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.AirSellFromRecommendation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IAddPnrMultiRepository
    {
        public Task<AddPnrMultiResponse> AddPnrMulti(AddPnrMultiRequset requestModel);
    }
}