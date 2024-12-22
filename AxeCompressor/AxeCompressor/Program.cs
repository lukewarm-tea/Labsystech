using AxeCompressor;

var simpleSerder = new SimpleSerder();
var bitSerder = BitRechunkingSerder.Default;
var deltaSerder = DeltaEncodingSerder.Default;
var presenter = new CompressionReportPresenter(Console.Out, [simpleSerder, bitSerder, deltaSerder]);
presenter.AnalyzeAndPresent();
