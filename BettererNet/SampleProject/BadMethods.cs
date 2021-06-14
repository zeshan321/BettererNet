using System.Threading.Tasks;

namespace SampleProject
{
    public class BadMethods
    {
        private async Task<int> BadMethod()
        {
            await Task.Delay(1000);
            return 4;
        }
        
        public async Task<int> BadMethod2()
        {
            await Task.Delay(1000);
            return 4;
        }
        
        protected async Task<int> BadMethod3()
        {
            await Task.Delay(1000);
            return 4;
        }
        
        protected async Task<int> BadMethod4()
        {
            await Task.Delay(1000);
            return 4;
        }

        public async Task<int> GoodMethodAsync()
        {
            await Task.Delay(1000);
            return 4;
        }
    }
}