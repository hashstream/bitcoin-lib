# bitcoin-lib
Bitcoin P2P Implementation in C# 

Generally speaking, anything on Consensus or API/RPC layer is not implemented yet.
The main purpose of this lib is for parsing and serializing data for the P2P network.

hashstream/libconsensus will be created in order to verify transactions / blocks.
hashstream/node will be created to act as a full bitcoin node (this is partially implemted in here for testing purposes) 

The below list applies to all bitcoin related libs in this org.

## BIP's implemented (Final / Proposed)
- [ ] BIP 0011 (M-of-N Standard Transactions)
- [x] BIP 0013 (Address Format for pay-to-script-hash)
- [x] BIP 0014 (Protocol Version and User Agent)
- [ ] BIP 0016 (Pay to Script Hash)
- [ ] BIP 0021 (URI Scheme)
- [ ] BIP 0022 (getblocktemplate - Fundamentals)
- [ ] BIP 0023 (getblocktemplate - Pooled Mining)
- [ ] BIP 0030 (Duplicate transactions)
- [x] BIP 0031 (Pong message)
- [ ] BIP 0032 (Hierarchical Deterministic Wallets)
- [ ] BIP 0034 (Block v2, Height in Coinbase)
- [ ] BIP 0035 (mempool message)
- [ ] BIP 0037 (Connection Bloom filtering)
- [ ] BIP 0061 (Reject P2P message)
- [ ] BIP 0065 (OP_CHECKLOCKTIMEVERIFY)
- [ ] BIP 0066 (Strict DER signatures)
- [ ] BIP 0068 (Relative lock-time using consensus-enforced sequence numbers)
- [ ] BIP 0070 (Payment Protocol)
- [ ] BIP 0071 (Payment Protocol MIME types)
- [ ] BIP 0072 (bitcoin: uri extensions for Payment Protocol)
- [ ] BIP 0073 (Use "Accept" header for response type negotiation with Payment Request URLs)
- [ ] BIP 0111 (NODE_BLOOM service bit)
- [ ] BIP 0112 (CHECKSEQUENCEVERIFY)
- [ ] BIP 0113 (Median time-past as endpoint for lock-time calculations)
- [ ] BIP 0125 (Opt-in Full Replace-by-Fee Signaling)
- [ ] BIP 0130 (sendheaders message)
- [ ] BIP 0141 (Segregated Witness (Consensus layer))
- [ ] BIP 0143 (Transaction Signature Verification for Version 0 Witness Program)
- [ ] BIP 0144 (Segregated Witness (Peer Services))
- [ ] BIP 0145 (getblocktemplate Updates for Segregated Witness)
- [ ] BIP 0147 (Dealing with dummy stack element malleability)
- [ ] BIP 0148 (Mandatory activation of segwit deployment)
- [x] BIP 0173 (Base32 address format for native v0-16 witness outputs)