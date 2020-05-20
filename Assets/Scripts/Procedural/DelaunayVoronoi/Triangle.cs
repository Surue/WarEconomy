using System.Collections.Generic;

namespace Procedural {
public sealed class Triangle {
    List<Site> sites_;

    public List<Site> Sites => sites_;

    public Triangle(Site a, Site b, Site c) {
        sites_ = new List<Site>(){ a, b, c};
    }

    public void Dispose() {
        sites_.Clear();
        sites_ = null;
    }
}
}
