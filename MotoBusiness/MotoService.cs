using MotoModel.DTOs; // PagedResult
using MotoModel.Entities;
using MotoData;
using Microsoft.EntityFrameworkCore;
using MotoBusiness.Exceptions; // ValidationException, BusinessException, NotFoundException

namespace MotoBusiness
{
    public class MotoService
    {
        private readonly AppDbContext _context;

        public MotoService(AppDbContext context)
        {
            _context = context;
        }

        // Método paginado
        public PagedResult<Moto> ListarTodas(int pageNumber, int pageSize)
        {
            var query = _context.Moto.AsQueryable();

            var totalItems = query.Count();

            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Moto>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public Moto ObterPorId(int id)
        {
            var moto = _context.Moto.Find(id);
            if (moto == null)
                throw new NotFoundException($"Moto com ID {id} não encontrada.");
            return moto;
        }

        public Moto? ObterPorTipo(string tipo) => _context.Moto.FirstOrDefault(m => m.tipoMoto == tipo);

        public Moto CadastrarMoto(Moto moto)
        {
            // Validações
            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(moto.tipoMoto))
                errors.Add("TipoMoto", new[] { "O campo TipoMoto é obrigatório." });
            if (string.IsNullOrWhiteSpace(moto.placa))
                errors.Add("Placa", new[] { "O campo Placa é obrigatório." });
            if (string.IsNullOrWhiteSpace(moto.numChassi))
                errors.Add("NumChassi", new[] { "O campo NumChassi é obrigatório." });

            if (errors.Count > 0)
                throw new ValidationException("Erro de validação", errors);

            try
            {
                _context.Moto.Add(moto);
                _context.SaveChanges();
                return moto;
            }
            catch (DbUpdateException ex)
            {
                throw new BusinessException("Erro ao salvar moto no banco de dados: " + ex.Message);
            }
        }


public bool Atualizar(int id, UpdateMoto motoDto)
{
    var existente = _context.Moto.Find(id);
    if (existente == null)
        throw new NotFoundException($"Moto com ID {id} não encontrada.");

    // Validações
    var errors = new Dictionary<string, string[]>();
    if (motoDto.TipoMoto != null && string.IsNullOrWhiteSpace(motoDto.TipoMoto))
        errors.Add("TipoMoto", new[] { "O campo TipoMoto não pode ser vazio." });

    if (motoDto.Placa != null && string.IsNullOrWhiteSpace(motoDto.Placa))
        errors.Add("Placa", new[] { "O campo Placa não pode ser vazio." });

    if (motoDto.NumChassi != null && string.IsNullOrWhiteSpace(motoDto.NumChassi))
        errors.Add("NumChassi", new[] { "O campo NumChassi não pode ser vazio." });

    if (errors.Count > 0)
        throw new ValidationException("Erro de validação", errors);

    try
    {
        // Atualiza apenas os campos enviados
        if (motoDto.TipoMoto != null) existente.tipoMoto = motoDto.TipoMoto;
        if (motoDto.Placa != null) existente.placa = motoDto.Placa;
        if (motoDto.NumChassi != null) existente.numChassi = motoDto.NumChassi;
        if (motoDto.TagRfidId.HasValue) existente.TagRfidId = motoDto.TagRfidId;

        _context.Moto.Update(existente);
        _context.SaveChanges();
        return true;
    }
    catch (DbUpdateException ex)
    {
        throw new BusinessException("Erro ao atualizar moto: " + ex.Message);
    }
}

        public bool Remover(int id)
        {
            var moto = _context.Moto.Find(id);
            if (moto == null)
                throw new NotFoundException($"Moto com ID {id} não encontrada.");

            try
            {
                _context.Moto.Remove(moto);
                _context.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new BusinessException("Erro ao remover moto: " + ex.Message);
            }
        }
    }
}
