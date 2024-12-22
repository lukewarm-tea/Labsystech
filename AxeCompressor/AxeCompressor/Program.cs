using AxeCompressor;

var console = Console.Out;
var simpleSerder = new SimpleSerder();
var bitSerder = BitRechunkingSerder.Default;
var deltaSerder = DeltaEncodingSerder.Default;
var presenter = new CompressionReportPresenter(console, [simpleSerder, bitSerder, deltaSerder]);
presenter.AnalyzeAndPresent();
