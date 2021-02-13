﻿using Flunt.Notifications;
using LocalizaLab.Operacoes.Application.Command;
using LocalizaLab.Operacoes.Application.Command.Carros;
using LocalizaLab.Operacoes.Domain.Command;
using LocalizaLab.Operacoes.Domain.Command.Handlers;
using LocalizaLab.Operacoes.Domain.Contracts.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using LocalizaLab.Operacoes.Domain.Entities;
using LocalizaLab.Operacoes.Application.Command.Modelos;
using LocalizaLab.Operacoes.Application.Command.Marca;
using LocalizaLab.Operacoes.Domain.Shared.Repository;
using LocalizaLab.Operacoes.Application.Presentation;
using System.Linq;

namespace LocalizaLab.Operacoes.Application.Handlers
{
    public class CarrosHandler : Notifiable, ICommandHandler<CadastrarVeiculoCommand>, ICommandHandler<CadastrarModeloCommand>,
                                                       ICommandHandler<CadastrarMarcaCommand>, ICommandHandler<DeletarVeiculoCommand>, ICommandHandler<EditarVeiculoCommand>
    {
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly IMarcaRepository _marcaRepository;
        private readonly IModeloRepository _modeloRepository;
        private readonly IMapper _mapper;

        public CarrosHandler(IVeiculoRepository veiculoRepository, IMapper mapper, IMarcaRepository marcaRepository, IModeloRepository modeloRepository)
        {
            _veiculoRepository = veiculoRepository;
            _mapper = mapper;
            _marcaRepository = marcaRepository;
            _modeloRepository = modeloRepository;
        }

        public async ValueTask<ICommandResult> Handle(CadastrarVeiculoCommand command)
        {
            if (!command.Validate())
            {
                AddNotifications(command);
                return new CommandResult(false, base.Notifications);
            }

            var entity = new Veiculos(command.Placa, command.Ano, command.ValorHora, command.Combustivel, command.LimitePortaMalas, command.Categoria,
                                      command.ModeloId);

            await _veiculoRepository.Add(entity);
            var result = await _veiculoRepository.SaveChanges().ConfigureAwait(true);

            if (!result) { return new CommandResult(false); }

            //base.BusinessError("");
            return new CommandResult(true);
        }
        public async ValueTask<ICommandResult> Handle(CadastrarMarcaCommand command)
        {
            if (!command.Validate())
            {
                AddNotifications(command);
                return new CommandResult(false, base.Notifications);
            }
            var entity = new Marca(command.Nome, command.Pais);

            await _marcaRepository.Add(entity);
            var result = await _marcaRepository.SaveChanges().ConfigureAwait(true);

            if (!result) { return new CommandResult(false); }

            return new CommandResult(true);
        }
        public async ValueTask<ICommandResult> Handle(CadastrarModeloCommand command)
        {
            if (!command.Validate())
            {
                AddNotifications(command);
                return new CommandResult(false, base.Notifications);
            }

            var entity = new Modelo(command.Nome, command.MarcaId);

            await _modeloRepository.Add(entity);
            var result = await _marcaRepository.SaveChanges().ConfigureAwait(true);

            if (!result) { return new CommandResult(false); }

            return new CommandResult(true);
        }
        public async ValueTask<ICommandResult> Handle(DeletarVeiculoCommand command)
        {
            if (!command.Validate())
            {
                AddNotifications(command);
                return new CommandResult(false, base.Notifications);
            }
            var result = await _veiculoRepository.GetById(command.Id).ConfigureAwait(true);
            if (result != null)
            {
                await _veiculoRepository.Remove(command.Id);
                await _veiculoRepository.SaveChanges().ConfigureAwait(true);
            }
            else
            {
                return new CommandResult(false, "Entidade Nao Encontrada.");
            }

            return new CommandResult(true);
        }
        public async ValueTask<ICommandResult> Handle(EditarVeiculoCommand command)
        {
            if (!command.Validate())
            {
                AddNotifications(command);
                return new CommandResult(false, base.Notifications);
            }

            var resultMarca = await AlterarMarca(command.Modelo.Marca).ConfigureAwait(true);
            var resultModelo = await AlterarModelo(command.Modelo).ConfigureAwait(true);
            if (resultMarca && resultModelo)
            {
                var entity = new Veiculos(command.Placa, command.Ano, command.ValorHora, command.Combustivel, command.LimitePortaMalas, command.Categoria, command.Modelo.Id);
                await _veiculoRepository.Update(entity);
                var result = await _veiculoRepository.SaveChanges().ConfigureAwait(true);
                return new CommandResult(true);

            }
            return new CommandResult(false,"Houve um erro ao atualizar os registros.");
        }
        private async Task<bool> AlterarMarca(EditarMarca marcarEditar)
        {
            var marcaEntity = await _marcaRepository.GetById(marcarEditar.Id).ConfigureAwait(true);

            if (marcaEntity != null)
            {
                marcaEntity.EditarMarca(marcarEditar.Nome, marcarEditar.Pais);
                await _marcaRepository.Update(marcaEntity).ConfigureAwait(true);
                await _marcaRepository.SaveChanges().ConfigureAwait(true);

                return true;
            }
            else
            {

                AddNotification("Marca", "Marca Nao encontrada para edicao.");
                return false;
            }

        }
        private async Task<bool> AlterarModelo(EditarModelo modeloEditar)
        {
            var modeloEntity = await _modeloRepository.GetById(modeloEditar.Id).ConfigureAwait(true);

            if (modeloEntity != null)
            {
                modeloEntity.AlterarNome(modeloEditar.Nome);
                await _modeloRepository.Update(modeloEntity).ConfigureAwait(true);
                await _modeloRepository.SaveChanges().ConfigureAwait(true);
                return true;
            }
            else
            {

                AddNotification("Modelo", "Modelo Nao encontrada para edicao.");
                return false;
            }

        }

    }

}
