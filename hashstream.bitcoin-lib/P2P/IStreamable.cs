namespace hashstream.bitcoin_lib.P2P
{
    public interface IStreamable
    {
        /// <summary>
        /// Reads the object off a byte[] buffer and returns its length
        /// </summary>
        /// <param name="data">The buffer to read from</param>
        /// <param name="offset">The offset in the buffer to start reading</param>
        /// <returns>Length of data read from buffer</returns>
        int ReadFromPayload(byte[] data, int offset);

        /// <summary>
        /// Serializes the object into a byte[]
        /// </summary>
        /// <returns>The serialized buffer</returns>
        byte[] ToArray();
    }
}
