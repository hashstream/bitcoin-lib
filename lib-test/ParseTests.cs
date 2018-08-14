using hashstream.bitcoin_lib;
using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib.Encoding;
using hashstream.bitcoin_lib.Script;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace lib_test
{
    public class ParseTests
    {

        private readonly ITestOutputHelper output;

        public ParseTests(ITestOutputHelper outputHelper)
        {
            output = outputHelper;
        }

        [Fact]
        public void Tx_Segwit_Parse()
        {
            var tx = "010000000001014bae8902e8dbc5260239c2af6e3b5ef066bbf1e17ecd82d9e28274573a66223a0400000000ffffffff0260e6e4650000000017a914483a524b6b66bbbb81f3954d4d622ae625a932d98750905e1100000000220020701a8d401c84fb13e6baf169d59684e17abd9fa216c8cc5b9fc63d622ff8c58d0400483045022100aa7b28cf7ad4ef3ee2b09b3526dc80357686400909fe2737c7bdeb4dc5e37cc6022022c45cc7aaa782a3466d6dce70e7fb37fdd7290b716d736259bfa399c6429daf01483045022100c27699aaddc48f02c742a6ea15400fd5d4607fdf2d1f4ea3a7cfb76c2e43a14302206ad6447e4db9c89d9c24ee8a41bb103a3aee08f9a42b7849803e11cb9a8348e2016952210266edd4ef2953675faf0662c088a7f620935807d200d65387290b31648e51e253210372ce38027ee95c98cdc54172964fa3aecf9f24b85c139d3d203365d6b691d0502103c96d495bfdd5ba4145e3e046fee45e84a8a48ad05bd8dbb395c011a32cf9f88053ae00000000".ToUpper().FromHex();

            var tx_parsed = new Tx();
            tx_parsed.ReadFromPayload(tx, 0);

            Assert.True(tx_parsed.Version == 1);

            Assert.True(tx_parsed.TxIn.Length == 1);
            Assert.True(tx_parsed.TxOut.Length == 2);

            Assert.True(tx_parsed.TxOut[0].Value == 1709500000);
            Assert.True(tx_parsed.TxOut[1].Value == 291410000);
        }

        [Fact]
        public void Tx_Parse()
        {
            var tx = "0100000001bb32aec76593327a638825be7fa38c1e48ff1bbe19f228963eb1737b248634d1010000006a47304402206bab89949b0aee41c3e6d548cad04784f35183d0d75c026566922e9dbb7d8434022005decf93695e7e88e8002758ae7605cb5d639f4bed6985c5c3150ed6af14c0f501210334bcc78a4bbe00b52c557ab2f4e0aa53c88cb52ce25882d287f37d0b2a5886adffffffff02f4b901000000000017a914068bccbacf3b33f71693ed1d59d409dad561f537876f96d521000000001976a914ee36e696e8ef973850f198a062bb8a3aee3fde9f88ac00000000".ToUpper().FromHex();

            var tx_parsed = new Tx();
            tx_parsed.ReadFromPayload(tx, 0);

            Assert.True(tx_parsed.Version == 1);

            Assert.True(tx_parsed.TxIn.Length == 1);
            Assert.True(tx_parsed.TxOut.Length == 2);

            Assert.True(tx_parsed.TxOut[0].Value == 113140);
            Assert.True(tx_parsed.TxOut[1].Value == 567645807);
        }

        [Fact]
        public void Tx_Out_Address()
        {
            //single output tests
            var tests = new string[][]
            {
                //P2PK
                new string[] { "12c6DSiU4Rq3P4ZxziKxzrL5LmMBrzjrJX", "01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff0704ffff001d0104ffffffff0100f2052a0100000043410496b538e853519c726a2c91e61ec11600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf621e73a82cbf2342c858eeac00000000"},
                //P2PKH
                new string[] { "18xRrVE5SGdD3QYgszxYyEjuCeHnfqMTqH", "0100000002144e211274abfee1ea9d63bb40002f36d6a0955d051649d460ac43c2631b6cae010000006b483045022100f0762d76613f2775445d6bd0e944c93e213d8b61aa314f56641c6692dc620a0f022077b6cec522f57ce3e21be954c3b0d24883fd946623a0fecfa44eceb5a6f14e9301210328582a9204abf0ee920f54c64adf5359b720fc44019b518ed214c997c531aac1ffffffff3dc9ace14aaf623bab12ea6ff7e8010954282654ab9042b18b913b621328726f000000006a473044022041d81ae917b556ab903e6a2d65af896de49e5ebc48ea4aba5a60dfae6b077d8d02202bddb8b3be71753c4fa9c22d418982566a80a657d8fccf46ab78b293bd615f8f012103184f0018baae12bf2499c4a868bfb9f05d94093596cfcc1dd505b366038da642ffffffff01f6537b0b000000001976a9145744813aff6759cc19595547e9947c21fc7c07f088ac00000000" },
                //P2SH
                new string[] { "3AZdx7cGMU1prFer7gzsmeNzERzo7pPSAP", "010000000162220eace15cc50893da8ec40a0aa8a4f890b60405f0d8ffcce3ceae1f65eda2030000006a4730440220516b1584bc1fcae77a6a04aada82b5663864e0f734a75d2a5bd643dbb271703802205cf4e92e9bb47bbd78484956a96b68f15701fface600182760a382e0a1afddb801210320d6ff1346469239bf7f147fcb8154b1dd22329d6409757c290a6762f8d6162cffffffff01e4f8a8000000000017a9146153e0d56d97dbbe56a5214a1289f6221ea6249a8700000000" },
                //P2SH_P2WPKH
                new string[] { "bc1qc8jgyt0wc4tv3nym274gn4fwe58tlswe5uhdjz", "010000000001019b5d5dfeff3f451a456b5bab1d2930f34ce940bb3bbf5c0794ddf8fd98f7c9f004000000171600147826fe22125ae742c3b91e7e6e41a7d743c25de1ffffffff02eb68000000000000160014c1e4822deec556c8cc9b57aa89d52ecd0ebfc1d99e790900000000001976a914153e989ec362cfdf14f46e8f3a986735dfc11cdd88ac02483045022100a0b07e0e0aa8a44f17b43bbd02045efa52224ac5aaac4db6ec08899dfc460c7d02200be3181a13dbc72a92777941c991cac0f01322563d34343404e4d0a2cedb9aec012103424b3498226467f8b015fe5c7d5f2e002c265242d0b3c3533f74a38b019580d200000000" },
                //P2WSH
                new string[] { "bc1qpapzchprzhnhg5kcamfm3vmfjlx49hzqjh5s2vrp38yst6fm9rkqmnhq3a", "0200000000010130c62a0fa62608f378b13645b89ef15ffce557414a35a251727dc3cf8a12aa050100000000a70d9980010e8a0700000000002200200f422c5c2315e77452d8eed3b8b36997cd52dc4095e905306189c905e93b28ec0400483045022100ac56ad14a790e0677ebceafc64d29a95eb9d6aa503bec173cdb57087d25f690302204b2e8dfd9e89f0d7917e423f50a49c86a786275cad320514ffefc4602723d8750147304402206e6b06e66a17d40674b93e47e11100c90ed8625231c1591ba66d92a958b8b9f00220734d487437d0084cd9fb5e7d15b2e5a87ab0bd25e495711c1bb69519fac3130c0147522102e6c7484ed0dd6b3632644c20b070dd64f1034fbb601206b6eaca08da8806bb81210365213c3d159cb62fe849f5ff2760d9bdb1784fdf362cb6d0eae1cdd4fe31369b52ae86125d20" }
            };

            foreach (var t in tests)
            {
                var tx = new Tx();
                tx.ReadFromPayload(t[1].FromHex(), 0);

                Assert.Equal(t[0], tx.TxOut[0].GetAddress().ToString());
            }
        }

        [Fact]
        public void Block_Parse()
        {
            var block_hash = "000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f";
            var block = "0100000000000000000000000000000000000000000000000000000000000000000000003ba3edfd7a7b12b27ac72c3e67768f617fc81bc3888a51323a9fb8aa4b1e5e4a29ab5f49ffff001d1dac2b7c0101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff4d04ffff001d0104455468652054696d65732030332f4a616e2f32303039204368616e63656c6c6f72206f6e206272696e6b206f66207365636f6e64206261696c6f757420666f722062616e6b73ffffffff0100f2052a01000000434104678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5fac00000000";

            var block_parsed = new Block();
            block_parsed.ReadFromPayload(block.FromHex(), 0);

            Assert.True(block_parsed.Header.Version == 1);
            Assert.Equal(block, block_parsed.ToArray().ToHex());
            Assert.Equal(block_hash, block_parsed.Hash.ToString());

            var satoshi = "The Times 03/Jan/2009 Chancellor on brink of second bailout for bank";

            Assert.Contains(satoshi, Encoding.UTF8.GetString((byte[])block_parsed.Txns[0].TxIn[0].Script.ScriptBytes));
        }

        [Fact]
        public void Block_Segwit_Parse()
        {
            var block = "000000209fbbf5595ad1ce45488bbf69f6e2aa887dcd1b7c2bac3400000000000000000054c1789b1b4703fbffda3ce5ef9c9eef75c039b835a96ea6a16f04efa67d52d408141b5b41f838174d96c4a707010000000001010000000000000000000000000000000000000000000000000000000000000000ffffffff33032f0908000406141b5b04148ddc0c08222932a1890500ff284d696e65642062792042572e434f4d29092f425720506f6f6c2fffffffff026daa814a000000001976a91404b4f2f410aaee6c0aaeb3144f7eba05f315a6d088ac0000000000000000266a24aa21a9ed8b9638071e3edfd5336ebdc1891a10793ec8ebbfd88d756f042f1da7170c4fdc01200000000000000000000000000000000000000000000000000000000000000000000000000200000001396a0f3ce5ddd0dd5eb725d8ad5e5f2f09f191846f4feb2d989f5acc2e4392a0010000006b483045022100a736b308763338e1b92c6c3ae1fc9f9185682b2b9fe4acb13bf2c465827721da022040a085fa66e910a15ab13e0c6867acf41ff4816916e04d1b02ca4d1d06059c0b0121020265992a07f2b2eb53955b3706952182041a7669363c3dab40b361486f7ede4affffffff0238c10100000000001976a914edd3f1bc6223922d9e39bb472170681b84012fce88acb3e2e800000000001976a9142a5c6cfe19a90048bd1ea887e081f7fd334f5b7a88ac000000000100000001b86731d35af87d48d5a2fa0a81a3b3b4dd7e366f048c2a66b20deb811e438445010000006b483045022100d73ab752db31665c70e3e8bb7d06130b671e06c21ce1d1ba9bf4bc118d192fef0220295c7d630578e1fedfc6a453507c5ca8cf3bb5fb2a24a1252ad5ed158bba8931012103ed7ae5e21a71630646fa910a7b65b30b057a6b4e21e0f509281aeb1f73e85aaaffffffff022284ff07000000001976a91451e17c2be670d0936207b8e912df2dc53939597088ac7c2c7500000000001976a914b4af8063b273a65766f697a1d8bf0791fe881d0788ac000000000100000001fc5e2d8877410693f69f5b493465ef1792b2c73f7f04eb5eeb8c65579aec6664010000006b4830450221008a9abd18ac063056ac1063b70dd6de6a4cc5224fcfc70f24786d128862cdc07d02200424a4178f473e598ee81cb98ae55a5d63cc9674d83dd37d599e61887f4209a20121034418e35ebb00a1f219ffe4fa772fdbd5cdf4033aa3af39b8563f5cc5f7ec2429ffffffff0200000000000000002b6a29e89d727546c797b61fb365cc0c78c5f342e8a1cedae7a61fb62a5450e8a978788fef33d60c943a0451a35b0700000000001976a9144be2d0c8f1a14b9a29cb28d088539c0265570a2f88ac0000000001000000013eb20dfa3298bb2ff11cc67f0de8201c9b990df10ee06fca0a30a0eeafcafd40010000006a473044022013e46169bc114659a4fc30f84028777cab1a3dc1a4cc096732a86e83205f1fae022002e073a14d267eb9feab61b3d626c0db973e3453d0bc9b3e8e12ee22de6ea7270121027169da28ecf3d7e8569568d5c0d88aaf7e89df09247829bcccb14a604742b1f8ffffffff02cc8104000000000017a9147295e87f55e63ce2971e77d3539d4c7e2b6724b087cc4b7c03000000001976a9145b527f22ac9b93ceb0caf3ddd3ddf0674cab72da88ac000000000200000001f211844bcc3997115e61ccc6c48a6d82af6ec5bebbaca842c9dd3a60a15027c9010000006a4730440220496668d8c377a046a7bf921a8924eec49686aff0e7c43d6237b06fb896a73dfa02207e92feebfeb090ac6ee15b8bd29535cee09454a5ae6d892daad5dc2e3ae10c3b012102185685dfc9310bb62d6d611cb834d3a964d04832e2701f52f2bbb1f660378661ffffffff0265660100000000001976a91497bea5cfcc636bd74c1e6dae1503a0512c6cdd2488ac91e05000000000001976a91470b2c761101116247d426e22f3f7c721b7c450d488ac000000000100000001c887bb13c18e3f2bbebbfceb39d66b585aae1e71e3c5eef535eee85488b71463010000006a4730440220737184236338bb896b8089c54b08bc2293d35514954d7a04d653dbdd5c8e582d0220507ba2dc27c01cc93097b117bd09ab7e2b1e87d4b36c6f1054d97ab250083aa80121039d594ed6c33d972937b260c0d0552272085b449b071f438f7eff4068115e9a4bffffffff02382b1a00000000001976a9140bba674694313939896305fb2d53f281f800c4e888ac0c473400000000001976a914d0966a55d617abf03ee1c0895bb768b5eb1c376e88ac00000000";

            var roffset = 0;
            var block_parsed = block.FromHex().ReadFromBuffer<Block>(ref roffset);

            Assert.True(block_parsed.Header.Version == 536870912);
            Assert.True(block_parsed.Txns.Length == 7);
            Assert.True(block_parsed.Txns[6].TxOut.Length == 2);
            Assert.True(block_parsed.Txns[6].TxOut[0].Value == 1715000);
            Assert.True(block_parsed.Txns[6].TxOut[1].Value == 3426060);
            Assert.True(block_parsed.Header.MerkleRoot == "d4527da6ef046fa1a66ea935b839c075ef9e9cefe53cdafffb03471b9b78c154");

            Assert.Equal(block, block_parsed.ToArray().ToHex());
        }

        [Fact]
        public void Bech32_Parse()
        {
            //from BIP 0173
            var main_p2wpkh = "bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4";
            var test_p2wpkh = "tb1qw508d6qejxtdg4y5r3zarvary0c5xw7kxpjzsx";
            var main_p2wsh = "bc1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3qccfmv3";
            var test_p2wsh = "tb1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3q0sl5k7";

            //check for exceptions
            Bech32.Decode(main_p2wpkh);
            Bech32.Decode(test_p2wpkh);
            Bech32.Decode(main_p2wsh);
            Bech32.Decode(test_p2wsh);

            //verify checksum fails                  ↓ this 'a' is supposed to be 'r'
            var check_fail = "bc1qw508d6qejxtdg4y5r3aarvary0c5xw7kv8f3t4";
            Assert.ThrowsAny<Exception>(() => Bech32.Decode(check_fail));
        }

        [Fact]
        public void Bech32Address_Decode()
        {
            var bc1 = new Bech32Address("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4");

            Assert.Equal("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4", bc1.ToString());
        }

        [Fact]
        public void Bech32Address_Encode()
        {
            var addr = "bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4";
            var hex_prog = "751e76e8199196d454941c45d1b3a323f1433bd6";
            var bc2 = new Bech32Address(AddressNetwork.Main, 0, hex_prog.FromHex());

            Assert.Equal(addr, bc2.ToString());
        }

        [Fact]
        public void Base58Address_Encode()
        {
            var tests = new Tuple<string, Base58Address>[]
            {
                Tuple.Create("17H3TJkbCgQSmmMpgAWjG559t3jkgV6Em6", new Base58Address(AddressNetwork.Main, Base58AddressType.PubKey, "44d9754d836e2021d814c9fe6e97770ab3f614cd".FromHex())),
                Tuple.Create("3FTWMqSqjXz8xp1hswKLBvqaFksHUGjUZE", new Base58Address(AddressNetwork.Main, Base58AddressType.Script, "9703acbc41aca13c5cd69a62dee56fb2590a38af".FromHex()))
            };

            foreach(var t in tests)
            {
                Assert.Equal(t.Item1, t.Item2.ToString());
            }
        }

        [Fact]
        public void Base58Address_Decode()
        {
            var tests = new Tuple<Base58Address, string>[]
            {
                Tuple.Create(new Base58Address("17H3TJkbCgQSmmMpgAWjG559t3jkgV6Em6"), "44d9754d836e2021d814c9fe6e97770ab3f614cd"),
                Tuple.Create(new Base58Address("3FTWMqSqjXz8xp1hswKLBvqaFksHUGjUZE"), "9703acbc41aca13c5cd69a62dee56fb2590a38af")
            };

            foreach (var t in tests)
            {
                Assert.Equal(t.Item1.ToArray().ToHex(), t.Item2);
            }

            // '7' at the end instead of '6'
            Assert.ThrowsAny<Exception>(() => new Base58Address("17H3TJkbCgQSmmMpgAWjG559t3jkgV6Em7"));
            // 'T' replaced with '7' inside data part
            Assert.ThrowsAny<Exception>(() => new Base58Address("17H37JkbCgQSmmMpgAWjG559t3jkgV6Em6"));
        }

        [Fact]
        public void Script_Parse_Standard_RedeemScript()
        {
            //human readable tests
            var test_hr = new string[][]
            {
                //P2PKH
                new string[] { "76a91489abcdefabbaabbaabbaabbaabbaabbaabbaabba88ac", "OP_DUP OP_HASH160 89abcdefabbaabbaabbaabbaabbaabbaabbaabba OP_EQUALVERIFY OP_CHECKSIG" },
                //P2SH
                new string[] { "a9143130913658056d961c7d73b0ce32e1f2ab565ea887", "OP_HASH160 3130913658056d961c7d73b0ce32e1f2ab565ea8 OP_EQUAL" },
                //coinbase output from block height 1 (P2PK)
                new string[] { "410496b538e853519c726a2c91e61ec11600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf621e73a82cbf2342c858eeac", "0496b538e853519c726a2c91e61ec11600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf621e73a82cbf2342c858ee OP_CHECKSIG" },
                //P2WSH
                new string[] { "0020701a8d401c84fb13e6baf169d59684e17abd9fa216c8cc5b9fc63d622ff8c58d", "OP_0 701a8d401c84fb13e6baf169d59684e17abd9fa216c8cc5b9fc63d622ff8c58d" }
            };
           
            foreach(var hrt in test_hr)
            {
                var cs = new Script(hrt[0].FromHex());

                Assert.Equal(hrt[1], cs.ToString());
            }
        }

        [Fact]
        public void Script_Witness_Verify()
        {
            var prev_tx = "01000000000101143663e4d2cdb0c4c4be480c1ba02c9983a9432af4549d0a79e7f600558e3ed10600000000ffffffff090084d7170000000017a914dcd19ff6b8cf942d558835f30ee49bf0c58cafbc8760892500000000001976a914d33d34d68a428c6b90a23a6eb106b8425d97942288ac0075a800000000001976a9142bf905f888c27babdf00b763a2ae59c74661633e88ac40771b00000000001976a914576cb1215cd96fd944ae2d65cfd6007918b664d588ace0673500000000001976a91422b32cd4ebf5940b0be060dec1d1948acd7a7ad488ac404b4c00000000001976a914b7c7e27153f41acd2c82f969b30b697e7877996688ac00093d000000000017a9142fe71b630bd770a12f7730a9d802751d523d038487a0072b00000000001976a91454a502e84da171584e31dfae7103039106f7017388ac0034891000000000220020701a8d401c84fb13e6baf169d59684e17abd9fa216c8cc5b9fc63d622ff8c58d040047304402201f589ebdee367c09bc8130170a4d8628fc92acae0113f1f078ed0acaa519c81a02200b8164f75949585317464cfe5ef9bb0480bd6236f39cff869d511e34a3b2eaa20147304402200af5e964046da3fc0a5064cddc95c7d044e015af131badaa27b1555776c40bcc02207b1b55731f10a084f9c39333fce454d614646efe4f635bf3b5f76e829b1bcac9016952210375e00eb72e29da82b89367947f29ef34afb75e8654f6ea368e0acdfd92976b7c2103a1b26313f430c4b15bb1fdce663207659d8cac749a0e53d70eff01874496feff2103c96d495bfdd5ba4145e3e046fee45e84a8a48ad05bd8dbb395c011a32cf9f88053ae00000000".FromHex();
            var prev_tx_parsed = new Tx();
            prev_tx_parsed.ReadFromPayload(prev_tx, 0); //vout 8 in this case for the below tx

            // 2-of-3 multisig P2WSH
            //txid: bb52a350bb27191522573d1d5b79cf5f503c918e05988b04085774dc816c444d
            var tx = "0100000000010139e82f99965dce11ee96fbe484f9f31cb7854910100734b205708809bc31a9480800000000ffffffff02c0336c04000000001976a9140185b799d6b09d74515f6b6101798000e190308888acb0a01b0c00000000220020701a8d401c84fb13e6baf169d59684e17abd9fa216c8cc5b9fc63d622ff8c58d0400483045022100b054fb57517fea8a7806f60510c8d94e86e1f66e33ffb80ca5ff41e31abb72a202207d72428da2aef2f04a9b0af1e5bae8245595441de0995dbb3caa082c29a2c2ca0147304402203b873c6b048295a114e90d31a9f4a78a6e03a9caf94f14d05542b8d17975671c02200532f3b4653bcf72f2c3c39629c87af131949fbdcad5add868a9628990734084016952210375e00eb72e29da82b89367947f29ef34afb75e8654f6ea368e0acdfd92976b7c2103a1b26313f430c4b15bb1fdce663207659d8cac749a0e53d70eff01874496feff2103c96d495bfdd5ba4145e3e046fee45e84a8a48ad05bd8dbb395c011a32cf9f88053ae00000000".FromHex();
            var tx_parsed = new Tx();
            tx_parsed.ReadFromPayload(tx, 0);
            

        }

        [Fact]
        public void Script_Basic_Tests()
        {
            //scriptdata, result
            //CScriptFrame can be (OpCode/int32/byte[]/string)
            var tests = new Dictionary<byte[], ScriptFrame>()
            {
                //OP_1 OP_1 OP_1ADD OP_ADD OP_3 OP_NUMEQUAL
                { "51518b93539c".FromHex(), 1 }, 
                //"hello world!" OP_RIPEMD160 <hash> OP_EQUAL
                { "0c68656c6c6f20776f726c6421a614dffd03137b3a333d5754813399a5f437acd694e587".FromHex(), 1 },
                //"hello world!" OP_SHA256 <hash> OP_EQUAL
                { "0c68656c6c6f20776f726c6421a8207509e5bda0c762d2bac7f90d758b5b2263fa01ccbc542ab5e3df163be08e6ca987".FromHex(), 1 }
            };

            foreach(var t in tests)
            {
                var cs = new Script(t.Key);
                output.WriteLine($"Testing script: {cs.ToString()}");

                var res = cs.ValidateParsedScript(cs.ParsedScript);
                
                Assert.Equal(t.Value, res);
            }
        }
        
        [Fact]
        public void Script_Bitcoin_Tests()
        {
            //https://raw.githubusercontent.com/bitcoin/bitcoin/master/src/test/data/tx_valid.json
        }

#if WITH_LIVE_TEST_TEST
        [Fact]
        public void Block_Parse_Sample()
        {
            var test_block = File.ReadAllBytes("test.dat");
            //var test_block = File.ReadAllText("blk_535942").Trim().FromHex();
            var bt = new Block();
            bt.ReadFromPayload(test_block, 0);

            Assert.Equal("00000000000000000008c45cfa71c551cc3437d08f989e094e87096370cb5ebc", bt.Hash.ToString());
        }

        [Fact]
        public void Parse_Last_100_Blocks()
        {
            var best_block_hash = Util.GetBestBlockHash();

            for(var x = 0; x < 100; x++)
            {
                var block_n = Util.GetBlock(best_block_hash);

                var block_parsed = new Block();
                block_parsed.ReadFromPayload(block_n, 0);

                best_block_hash = block_parsed.Header.PrevBlock;
            }
        }
#endif
    }
}
