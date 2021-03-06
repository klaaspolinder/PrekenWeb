﻿using System.Collections.Generic;
using System.Threading.Tasks;
using App.Shared.Api;
using App.Shared.Db;
using Prekenweb.Models.Dtos;

namespace App.Shared.Services
{
    public class PreekService : IPreekService
    {
        private readonly IPrekenwebApiWrapper _prekenwebApiWrapper;
        private readonly IPrekenwebAppDatabase _prekenwebAppDatabase;

        public PreekService(IPrekenwebApiWrapper prekenwebApiWrapper, IPrekenwebAppDatabase prekenwebAppDatabase)
        {
            _prekenwebApiWrapper = prekenwebApiWrapper;
            _prekenwebAppDatabase = prekenwebAppDatabase;
        }

        public IEnumerable<PreekInLocalDb> GetNieuwePreken()
        {
            var localPreken = _prekenwebAppDatabase.GetNieuwePreken();

            return localPreken;
        }

        public async Task UpdatePreken()
        {
            var nieuwePreken = await _prekenwebApiWrapper.NieuwePreken();

            AutoMapper.Mapper.CreateMap<Preek, NieuwePreekInLocalDb>()
                .ForMember(x => x.Titel, y => y.MapFrom(z => z.PreekTitel))
                .ForMember(x => x.Filename, y => y.MapFrom(z => z.Bestandsnaam));

            var nieuwePrekenInDb = AutoMapper.Mapper.Map<IEnumerable<NieuwePreekInLocalDb>>(nieuwePreken);
            _prekenwebAppDatabase.UpdateNieuwePreken(nieuwePrekenInDb);
        }

        public Task SetLocalPreekFilename(int preekId, string localFilename)
        {
            var preek = _prekenwebAppDatabase.GetPreek(preekId);
            preek.LocalFilePath = localFilename;
            _prekenwebAppDatabase.SavePreek(preek);
            return Task.FromResult(true);
        }
    }
}
