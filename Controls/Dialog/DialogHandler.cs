using System.Threading.Tasks;

namespace Ion;

public delegate Task DialogClosedHandler(int result);

public delegate Task<int> DialogOpenedHandler();