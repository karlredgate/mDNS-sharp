
using mDNS = Redgate.Net.mDNS;

class Listen {
    public static void Main( string [] args ) {
        mDNS.Resolver client = new mDNS.Resolver();
	mDNS.Response response;
	while ( true ) {
	    response = client.Receive();
	    response.Write();
	}
    }
}
