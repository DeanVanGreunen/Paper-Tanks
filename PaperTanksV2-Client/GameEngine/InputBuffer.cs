using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class InputBuffer
    {
        private readonly Queue<PlayerInput> buffer;
        private readonly int maxBufferSize;
        private uint currentSequence;

        public InputBuffer(int bufferSize = 30)
        {
            buffer = new Queue<PlayerInput>();
            maxBufferSize = bufferSize;
            currentSequence = 0;
        }

        public void AddInput(PlayerInput input)
        {
            input.Sequence = ++currentSequence;
            buffer.Enqueue(input);

            // Maintain buffer size
            while (buffer.Count > maxBufferSize) {
                buffer.Dequeue();
            }
        }

        public PlayerInput[] GetInputsSince(uint sequence)
        {
            return buffer.Where(input => input.Sequence > sequence).ToArray();
        }

        public void ClearOlderThan(uint sequence)
        {
            while (buffer.Count > 0 && buffer.Peek().Sequence <= sequence) {
                buffer.Dequeue();
            }
        }
    }
}
