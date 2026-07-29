using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Arquivos;

public sealed class ArquivoArmazenado : Entidade
{
    public const int TamanhoMaximoDaChave = 500;
    public const int TamanhoMaximoDoNomeOriginal = 255;
    public const int TamanhoMaximoDoTipoDeConteudo = 100;
    public const int TamanhoMaximoDoETag = 200;
    public const int TamanhoMaximoDoErro = 1000;

    private ArquivoArmazenado()
    {
        ChaveDoObjeto = string.Empty;
        NomeOriginal = string.Empty;
        TipoDeConteudo = string.Empty;
    }

    private ArquivoArmazenado(
        Guid identificador,
        string chaveDoObjeto,
        FinalidadeDoArquivo finalidade,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoRecurso,
        Guid? identificadorDoEncontro,
        string nomeOriginal,
        string tipoDeConteudo,
        long tamanhoReservadoEmBytes,
        DateTimeOffset expiraEm,
        DateTimeOffset criadoEm)
        : base(identificador, criadoEm)
    {
        if (identificadorDoUsuarioResponsavel == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O usuário responsável pelo arquivo é obrigatório.");
        }

        if (!Enum.IsDefined(finalidade))
        {
            throw new ExcecaoDeDominioException("A finalidade do arquivo é inválida.");
        }

        if (identificadorDoRecurso == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O recurso associado ao arquivo é obrigatório.");
        }

        bool encontroEhObrigatorio = finalidade is FinalidadeDoArquivo.ImagemDeCapaDoEncontro
            or FinalidadeDoArquivo.MidiaDeMemoria;

        if (encontroEhObrigatorio && (!identificadorDoEncontro.HasValue || identificadorDoEncontro == Guid.Empty))
        {
            throw new ExcecaoDeDominioException("O encontro associado ao arquivo é obrigatório.");
        }

        if (!encontroEhObrigatorio && identificadorDoEncontro.HasValue)
        {
            throw new ExcecaoDeDominioException("A foto de perfil não deve estar associada a um encontro.");
        }

        if (tamanhoReservadoEmBytes <= 0)
        {
            throw new ExcecaoDeDominioException("O tamanho reservado deve ser maior que zero.");
        }

        if (expiraEm <= criadoEm)
        {
            throw new ExcecaoDeDominioException("A reserva do arquivo deve expirar depois da criação.");
        }

        ChaveDoObjeto = NormalizeTexto(chaveDoObjeto, TamanhoMaximoDaChave, "A chave do arquivo é obrigatória.");
        Finalidade = finalidade;
        IdentificadorDoUsuarioResponsavel = identificadorDoUsuarioResponsavel;
        IdentificadorDoRecurso = identificadorDoRecurso;
        IdentificadorDoEncontro = identificadorDoEncontro;
        NomeOriginal = NormalizeTexto(nomeOriginal, TamanhoMaximoDoNomeOriginal, "O nome original do arquivo é obrigatório.");
        TipoDeConteudo = NormalizeTexto(tipoDeConteudo, TamanhoMaximoDoTipoDeConteudo, "O tipo de conteúdo do arquivo é obrigatório.");
        TamanhoReservadoEmBytes = tamanhoReservadoEmBytes;
        Situacao = SituacaoDoArquivoArmazenado.Reservado;
        ExpiraEm = expiraEm;
    }

    public string ChaveDoObjeto { get; private set; }
    public FinalidadeDoArquivo Finalidade { get; private set; }
    public Guid IdentificadorDoUsuarioResponsavel { get; private set; }
    public Guid IdentificadorDoRecurso { get; private set; }
    public Guid? IdentificadorDoEncontro { get; private set; }
    public string NomeOriginal { get; private set; }
    public string TipoDeConteudo { get; private set; }
    public long TamanhoReservadoEmBytes { get; private set; }
    public long? TamanhoConfirmadoEmBytes { get; private set; }
    public string? ETag { get; private set; }
    public SituacaoDoArquivoArmazenado Situacao { get; private set; }
    public DateTimeOffset ExpiraEm { get; private set; }
    public DateTimeOffset? AtivadoEm { get; private set; }
    public DateTimeOffset? ExcluidoEm { get; private set; }
    public int TentativasDeExclusao { get; private set; }
    public string? UltimoErroDeExclusao { get; private set; }

    public static ArquivoArmazenado Reserve(
        Guid identificador,
        string chaveDoObjeto,
        FinalidadeDoArquivo finalidade,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoRecurso,
        Guid? identificadorDoEncontro,
        string nomeOriginal,
        string tipoDeConteudo,
        long tamanhoReservadoEmBytes,
        DateTimeOffset expiraEm,
        DateTimeOffset criadoEm)
    {
        return new(
            identificador,
            chaveDoObjeto,
            finalidade,
            identificadorDoUsuarioResponsavel,
            identificadorDoRecurso,
            identificadorDoEncontro,
            nomeOriginal,
            tipoDeConteudo,
            tamanhoReservadoEmBytes,
            expiraEm,
            criadoEm);
    }

    public void Ative(long tamanhoConfirmadoEmBytes, string? eTag, DateTimeOffset ativadoEm)
    {
        if (Situacao == SituacaoDoArquivoArmazenado.Ativo)
        {
            return;
        }

        if (Situacao != SituacaoDoArquivoArmazenado.Reservado)
        {
            throw new ExcecaoDeDominioException("Somente uma reserva pode ser ativada.");
        }

        if (tamanhoConfirmadoEmBytes <= 0 || tamanhoConfirmadoEmBytes > TamanhoReservadoEmBytes)
        {
            throw new ExcecaoDeDominioException("O tamanho confirmado é inválido para a reserva.");
        }

        TamanhoConfirmadoEmBytes = tamanhoConfirmadoEmBytes;
        ETag = NormalizeTextoOpcional(eTag, TamanhoMaximoDoETag);
        Situacao = SituacaoDoArquivoArmazenado.Ativo;
        AtivadoEm = ativadoEm;
    }

    public void Cancele()
    {
        if (Situacao == SituacaoDoArquivoArmazenado.Cancelado)
        {
            return;
        }

        if (Situacao != SituacaoDoArquivoArmazenado.Reservado)
        {
            throw new ExcecaoDeDominioException("Somente uma reserva pode ser cancelada.");
        }

        Situacao = SituacaoDoArquivoArmazenado.Cancelado;
    }

    public void Expire()
    {
        if (Situacao == SituacaoDoArquivoArmazenado.Expirado)
        {
            return;
        }

        if (Situacao != SituacaoDoArquivoArmazenado.Reservado)
        {
            throw new ExcecaoDeDominioException("Somente uma reserva pode expirar.");
        }

        Situacao = SituacaoDoArquivoArmazenado.Expirado;
    }

    public void MarqueExclusaoPendente()
    {
        if (Situacao == SituacaoDoArquivoArmazenado.ExclusaoPendente)
        {
            return;
        }

        if (Situacao != SituacaoDoArquivoArmazenado.Ativo)
        {
            throw new ExcecaoDeDominioException("Somente um arquivo ativo pode aguardar exclusão.");
        }

        Situacao = SituacaoDoArquivoArmazenado.ExclusaoPendente;
        UltimoErroDeExclusao = null;
    }

    public void RegistreFalhaNaExclusao(string erro)
    {
        if (Situacao != SituacaoDoArquivoArmazenado.ExclusaoPendente)
        {
            throw new ExcecaoDeDominioException("O arquivo não está aguardando exclusão.");
        }

        TentativasDeExclusao++;
        UltimoErroDeExclusao = NormalizeTexto(erro, TamanhoMaximoDoErro, "O erro da exclusão é obrigatório.");
    }

    public void ConfirmeExclusao(DateTimeOffset excluidoEm)
    {
        if (Situacao == SituacaoDoArquivoArmazenado.Excluido)
        {
            return;
        }

        if (Situacao != SituacaoDoArquivoArmazenado.ExclusaoPendente)
        {
            throw new ExcecaoDeDominioException("O arquivo não está aguardando exclusão.");
        }

        Situacao = SituacaoDoArquivoArmazenado.Excluido;
        ExcluidoEm = excluidoEm;
        UltimoErroDeExclusao = null;
    }

    private static string NormalizeTexto(string texto, int tamanhoMaximo, string mensagemObrigatoria)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new ExcecaoDeDominioException(mensagemObrigatoria);
        }

        string textoNormalizado = texto.Trim();

        if (textoNormalizado.Length > tamanhoMaximo)
        {
            throw new ExcecaoDeDominioException($"O texto não pode ultrapassar {tamanhoMaximo} caracteres.");
        }

        return textoNormalizado;
    }

    private static string? NormalizeTextoOpcional(string? texto, int tamanhoMaximo)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return null;
        }

        return NormalizeTexto(texto, tamanhoMaximo, "O texto é obrigatório.");
    }
}
