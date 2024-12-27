
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using QifApi;
using QifApi.Transactions;

namespace AppliedHyperkinetics.Qif.Driver
{
    /// <summary>
    /// Class for mapping Fidelity-provided asset (security) names to friendly values based on an internal mapping Dictionary.
    /// </summary>
    /// <remarks>
    /// Fidelity uses some funky asset names that don't match those downloaded by Quicken if you have a ticker symbol for them. 
    /// This class is specialized to map the assets we have. A more general 
    /// </remarks>
    public class AssetNameTranslator
    {
        private Dictionary<string, string> _assetNameMap = new Dictionary<string, string>()
        {
            // Kim's 401k assets
            { "FID 500 INDEX(2328)", "Fidelity 500 Index Fund"},
            { "FID MID CAP IDX(2352)", "Fidelity Mid Cap Index Fund"},
            // John's 401k assets
            { "VANG RUS 1000 VAL TR(OAMP)", "Vanguard Russell 1000 Value Index Fund Institutional Shares" },
            { "VANG 500 IDX IS SEL(OQZ1)", "Vanguard 500 Index Institutional Select Shares" },
            { "INTL VALUE ACCOUNT(TP12)", "International Value Fund" }
        };

        /// <summary>
        /// Given a QifDom object containing transactions, rewrite the Fidelity-provided asset name (InvestmentTransaction::Security)
        /// to a friendly name contained in <see cref="_assetNameMap"/> if such a friendly name is mapped
        /// and return the updated <see cref="QifDom"/> object. 
        /// </summary>
        /// <returns>
        /// <see cref="QifDom"/> object with the asset names remapped.
        /// </returns>
        public QifDom MapFidelityAssetNamesToDownloadedNames(QifDom inFile)
        {
            QifDom outFile = new QifDom();

            foreach (var tx in inFile.InvestmentTransactions)
            {
                var originalAssetName = tx.Security;
                var mappedAssetName = originalAssetName;
                if (_assetNameMap.ContainsKey(originalAssetName))
                {
                    mappedAssetName = _assetNameMap[originalAssetName];
                }

                var incTx = new InvestmentTransaction()
                {
                    Date = tx.Date,
                    Action = tx.Action,
                    Security = mappedAssetName,
                    Price = tx.Price,
                    Quantity = tx.Quantity,
                    TransactionAmount = tx.TransactionAmount,
                    Memo = tx.Memo += $" [Remapped asset name: {originalAssetName} -> {mappedAssetName}]"
                };

                outFile.InvestmentTransactions.Add(incTx);

                if (!originalAssetName.Equals(mappedAssetName))
                {
                    Console.WriteLine($"Mapped original asset name {originalAssetName} to {mappedAssetName} for {incTx.Action} dated {incTx.Date}");
                }
            }

            return outFile;
        }
    }
}