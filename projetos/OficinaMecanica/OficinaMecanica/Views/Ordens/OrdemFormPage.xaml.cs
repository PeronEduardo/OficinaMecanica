using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;

namespace OficinaMecanica.Views.Ordens;

[QueryProperty(nameof(OrdemId), "id")]
public partial class OrdemFormPage : ContentPage
{
    private readonly IOrdemServicoController _ordemController;
    private readonly IClienteController      _clienteController;
    private readonly IVeiculoController      _veiculoController;
    private readonly IServicoService         _servicoService;
    private readonly IPecaService            _pecaService;

    // Listas auxiliares para popular os Pickers
    private List<Cliente>  _clientes  = new();
    private List<Veiculo>  _veiculos  = new();
    private List<Servico>  _servicos  = new();
    private List<Peca>     _pecas     = new();

    // Listas de itens adicionados à OS
    private List<OrdemServicoServico> _servicosAdicionados = new();
    private List<OrdemServicoPeca>    _pecasAdicionadas    = new();

    // OS sendo editada (null = nova OS)
    private OrdemServico? _ordemExistente;

    public string? OrdemId
    {
        set
        {
            if (int.TryParse(value, out int id) && id > 0)
                CarregarOrdemExistenteAsync(id).ConfigureAwait(false);
        }
    }

    public OrdemFormPage(
        IOrdemServicoController ordemController,
        IClienteController clienteController,
        IVeiculoController veiculoController,
        IServicoService servicoService,
        IPecaService pecaService)
    {
        InitializeComponent();
        _ordemController   = ordemController;
        _clienteController = clienteController;
        _veiculoController = veiculoController;
        _servicoService    = servicoService;
        _pecaService       = pecaService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarDadosIniciaisAsync();
    }

    // Carrega clientes, serviços e peças para popular os Pickers.
    private async Task CarregarDadosIniciaisAsync()
    {
        // Clientes
        var resultClientes = await _clienteController.ListarAsync();
        if (resultClientes.Sucesso)
        {
            _clientes = resultClientes.Dados ?? new();
            PickerCliente.ItemsSource       = _clientes;
            PickerCliente.ItemDisplayBinding = new Binding("Nome");
        }

        // Serviços ativos
        _servicos = await _servicoService.ListarAtivosAsync();
        PickerServico.ItemsSource       = _servicos;
        PickerServico.ItemDisplayBinding = new Binding("Descricao");

        // Peças ativas
        _pecas = await _pecaService.ListarAtivosAsync();
        PickerPeca.ItemsSource       = _pecas;
        PickerPeca.ItemDisplayBinding = new Binding("Descricao");
    }

    // Quando o cliente é selecionado, carrega os veículos dele.
    private async void OnClienteSelecionado(object sender, EventArgs e)
    {
        if (PickerCliente.SelectedItem is not Cliente cliente) return;

        PickerVeiculo.IsEnabled = false;
        PickerVeiculo.SelectedItem = null;

        var resultado = await _veiculoController.ListarPorClienteAsync(cliente.Id);

        if (resultado.Sucesso && resultado.Dados?.Any() == true)
        {
            _veiculos = resultado.Dados;
            PickerVeiculo.ItemsSource        = _veiculos;
            PickerVeiculo.ItemDisplayBinding = new Binding("PlacaModelo");
            PickerVeiculo.IsEnabled          = true;
        }
        else
        {
            await DisplayAlertAsync("Atenção",
                "Este cliente não possui veículos cadastrados.\n" +
                "Cadastre um veículo antes de abrir uma OS.", "OK");
        }
    }

    // Adiciona serviço selecionado à lista da OS.
    private void OnAdicionarServicoClicked(object sender, EventArgs e)
    {
        if (PickerServico.SelectedItem is not Servico servico) return;

        // Evita duplicidade na lista.
        if (_servicosAdicionados.Any(s => s.ServicoId == servico.Id))
        {
            DisplayAlert("Atenção", "Este serviço já foi adicionado.", "OK");
            return;
        }

        _servicosAdicionados.Add(new OrdemServicoServico
        {
            ServicoId      = servico.Id,
            Servico        = servico,
            Quantidade     = 1,
            PrecoUnitario  = servico.Preco
        });

        // Força atualização da CollectionView com nova lista.
        ListaServicosAdicionados.ItemsSource = null;
        ListaServicosAdicionados.ItemsSource = _servicosAdicionados;

        AtualizarTotais();
        PickerServico.SelectedItem = null;
    }

    // Adiciona peça selecionada à lista da OS.
    private void OnAdicionarPecaClicked(object sender, EventArgs e)
    {
        if (PickerPeca.SelectedItem is not Peca peca) return;

        if (_pecasAdicionadas.Any(p => p.PecaId == peca.Id))
        {
            DisplayAlert("Atenção", "Esta peça já foi adicionada.", "OK");
            return;
        }

        _pecasAdicionadas.Add(new OrdemServicoPeca
        {
            PecaId        = peca.Id,
            Peca          = peca,
            Quantidade    = 1,
            PrecoUnitario = peca.Preco
        });

        ListaPecasAdicionadas.ItemsSource = null;
        ListaPecasAdicionadas.ItemsSource = _pecasAdicionadas;

        AtualizarTotais();
        PickerPeca.SelectedItem = null;
    }

    // Remove serviço da lista.
    private void OnRemoverServicoClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is OrdemServicoServico item)
        {
            _servicosAdicionados.Remove(item);
            ListaServicosAdicionados.ItemsSource = null;
            ListaServicosAdicionados.ItemsSource = _servicosAdicionados;
            AtualizarTotais();
        }
    }

    // Remove peça da lista.
    private void OnRemoverPecaClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is OrdemServicoPeca item)
        {
            _pecasAdicionadas.Remove(item);
            ListaPecasAdicionadas.ItemsSource = null;
            ListaPecasAdicionadas.ItemsSource = _pecasAdicionadas;
            AtualizarTotais();
        }
    }

    // Recalcula e exibe os totais na tela.
    private void AtualizarTotais()
    {
        var subtotalServicos = _servicosAdicionados.Sum(s => s.Subtotal);
        var subtotalPecas    = _pecasAdicionadas.Sum(p => p.Subtotal);

        decimal desconto = 0;
        if (decimal.TryParse(
            EntryDesconto.Text?.Replace(",", "."),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out decimal d))
            desconto = d;

        var total = Math.Max(0, subtotalServicos + subtotalPecas - desconto);

        LabelSubtotalServicos.Text = $"R$ {subtotalServicos:F2}";
        LabelSubtotalPecas.Text    = $"R$ {subtotalPecas:F2}";
        LabelTotal.Text            = $"R$ {total:F2}";
    }

    // Recalcula ao alterar o desconto.
    private void OnDescontoChanged(object sender, TextChangedEventArgs e)
        => AtualizarTotais();

    // Salva a OS completa.
    private async void OnSalvarClicked(object sender, EventArgs e)
    {
        LabelErro.IsVisible  = false;
        Carregando.IsVisible = true;
        Carregando.IsRunning = true;
        BtnSalvar.IsEnabled  = false;

        // Validação visual antes de montar o objeto.
        if (PickerCliente.SelectedItem is not Cliente cliente)
        {
            MostrarErro("Selecione um cliente.");
            return;
        }

        if (PickerVeiculo.SelectedItem is not Veiculo veiculo)
        {
            MostrarErro("Selecione um veículo.");
            return;
        }

        if (!_servicosAdicionados.Any() && !_pecasAdicionadas.Any())
        {
            MostrarErro("Adicione pelo menos um serviço ou peça.");
            return;
        }

        decimal desconto = 0;
        decimal.TryParse(
            EntryDesconto.Text?.Replace(",", "."),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out desconto);

        // Monta o objeto OrdemServico com todos os itens.
        var ordem = _ordemExistente ?? new OrdemServico();
        ordem.ClienteId            = cliente.Id;
        ordem.VeiculoId            = veiculo.Id;
        ordem.UsuarioId            = Login.SessaoUsuario.UsuarioAtual?.Id ?? 1;
        ordem.DataAbertura         = _ordemExistente?.DataAbertura ?? DateTime.Today;
        ordem.DataPrevisao         = DatePrevisao.Date;
        ordem.QuilometragemEntrada = int.TryParse(EntryKm.Text, out int km) ? km : null;
        ordem.Observacoes          = EditorObservacoes.Text?.Trim();
        ordem.Desconto             = desconto;
        ordem.Servicos             = _servicosAdicionados;
        ordem.Pecas                = _pecasAdicionadas;

        var resultado = await _ordemController.SalvarAsync(ordem);

        Carregando.IsRunning = false;
        Carregando.IsVisible = false;
        BtnSalvar.IsEnabled  = true;

        if (!resultado.Sucesso)
        {
            MostrarErro(resultado.Mensagem);
            return;
        }

        await DisplayAlertAsync("Sucesso", resultado.Mensagem, "OK");
        await Navigation.PopAsync();
    }

    // Carrega os dados de uma OS existente para edição.
    private async Task CarregarOrdemExistenteAsync(int id)
    {
        var resultado = await _ordemController.BuscarCompletoAsync(id);
        if (!resultado.Sucesso || resultado.Dados is null) return;

        _ordemExistente          = resultado.Dados;
        _servicosAdicionados     = _ordemExistente.Servicos;
        _pecasAdicionadas        = _ordemExistente.Pecas;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Title = $"Editar OS #{id}";

            // Seleciona o cliente no Picker.
            var clienteIndex = _clientes.FindIndex(c => c.Id == _ordemExistente.ClienteId);
            if (clienteIndex >= 0) PickerCliente.SelectedIndex = clienteIndex;

            // Preenche demais campos.
            if (_ordemExistente.DataPrevisao.HasValue)
                DatePrevisao.Date = _ordemExistente.DataPrevisao.Value;

            EntryKm.Text           = _ordemExistente.QuilometragemEntrada?.ToString();
            EntryDesconto.Text     = _ordemExistente.Desconto.ToString("F2");
            EditorObservacoes.Text = _ordemExistente.Observacoes;

            // Atualiza as listas de itens.
            ListaServicosAdicionados.ItemsSource = _servicosAdicionados;
            ListaPecasAdicionadas.ItemsSource    = _pecasAdicionadas;

            AtualizarTotais();
        });
    }

    private void MostrarErro(string mensagem)
    {
        Carregando.IsRunning = false;
        Carregando.IsVisible = false;
        BtnSalvar.IsEnabled  = true;
        LabelErro.Text       = mensagem;
        LabelErro.IsVisible  = true;
    }
}
