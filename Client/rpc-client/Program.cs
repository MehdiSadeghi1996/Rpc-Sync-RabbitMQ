


using rpc_client;

var queue = "rpcexamplequeue";

do
{
    var rpcClient = new RpcClinet(queue);

    var response = rpcClient.Call("30");

    Console.WriteLine("response recived : ", response);
    rpcClient.Close();

    Console.WriteLine("Do it again? Y/n");
} 
while
(Char.ToUpper(Console.ReadKey().KeyChar) == 'Y');
