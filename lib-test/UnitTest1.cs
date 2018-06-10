using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib;
using System;
using System.Diagnostics;
using Xunit;

namespace lib_test
{
    public class UnitTest1
    {
        [Fact]
        public void Test_Witness_TX_Parse()
        {
            var tx = "010000000001014bae8902e8dbc5260239c2af6e3b5ef066bbf1e17ecd82d9e28274573a66223a0400000000ffffffff0260e6e4650000000017a914483a524b6b66bbbb81f3954d4d622ae625a932d98750905e1100000000220020701a8d401c84fb13e6baf169d59684e17abd9fa216c8cc5b9fc63d622ff8c58d0400483045022100aa7b28cf7ad4ef3ee2b09b3526dc80357686400909fe2737c7bdeb4dc5e37cc6022022c45cc7aaa782a3466d6dce70e7fb37fdd7290b716d736259bfa399c6429daf01483045022100c27699aaddc48f02c742a6ea15400fd5d4607fdf2d1f4ea3a7cfb76c2e43a14302206ad6447e4db9c89d9c24ee8a41bb103a3aee08f9a42b7849803e11cb9a8348e2016952210266edd4ef2953675faf0662c088a7f620935807d200d65387290b31648e51e253210372ce38027ee95c98cdc54172964fa3aecf9f24b85c139d3d203365d6b691d0502103c96d495bfdd5ba4145e3e046fee45e84a8a48ad05bd8dbb395c011a32cf9f88053ae00000000".ToUpper().FromHex();

            var tx_parsed = new Tx();
            tx_parsed.ReadFromPayload(tx, 0);

            Debug.Assert(tx_parsed.Version == 1);

            Debug.Assert(tx_parsed.TxIn.Length == 1);
            Debug.Assert(tx_parsed.TxOut.Length == 2);

            Debug.Assert(tx_parsed.TxOut[0].Value == 1709500000);
            Debug.Assert(tx_parsed.TxOut[1].Value == 291410000);
        }

        [Fact]
        public void Test_TX_Parse()
        {
            var tx = "0100000001bb32aec76593327a638825be7fa38c1e48ff1bbe19f228963eb1737b248634d1010000006a47304402206bab89949b0aee41c3e6d548cad04784f35183d0d75c026566922e9dbb7d8434022005decf93695e7e88e8002758ae7605cb5d639f4bed6985c5c3150ed6af14c0f501210334bcc78a4bbe00b52c557ab2f4e0aa53c88cb52ce25882d287f37d0b2a5886adffffffff02f4b901000000000017a914068bccbacf3b33f71693ed1d59d409dad561f537876f96d521000000001976a914ee36e696e8ef973850f198a062bb8a3aee3fde9f88ac00000000".ToUpper().FromHex();

            var tx_parsed = new Tx();
            tx_parsed.ReadFromPayload(tx, 0);

            Debug.Assert(tx_parsed.Version == 1);

            Debug.Assert(tx_parsed.TxIn.Length == 1);
            Debug.Assert(tx_parsed.TxOut.Length == 2);

            Debug.Assert(tx_parsed.TxOut[0].Value == 113140);
            Debug.Assert(tx_parsed.TxOut[1].Value == 567645807);
        }
    }
}
