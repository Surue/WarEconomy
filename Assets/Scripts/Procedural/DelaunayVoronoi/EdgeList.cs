using UnityEngine;

namespace Procedural {
sealed class EdgeList {
    float deltaX_;
    float xMin_;

    int hashSize_;
    HalfEdge[] hash_;
    HalfEdge leftEnd_;
    public HalfEdge LeftEnd => leftEnd_;

    HalfEdge rightEnd_;

    public HalfEdge RightEnd => rightEnd_;
    
    public EdgeList(float xMin, float deltaX, int sqrtNSite) {
        xMin_ = xMin;
        deltaX_ = deltaX;
        hashSize_ = 2 * sqrtNSite;
        
        hash_ = new HalfEdge[hashSize_];
        
        leftEnd_ = HalfEdge.CreateDummy ();
        rightEnd_ = HalfEdge.CreateDummy ();
        
        leftEnd_.edgeListLeftNeighbor = null;
        leftEnd_.edgeListRightNeighbor = rightEnd_;
        rightEnd_.edgeListLeftNeighbor = leftEnd_;
        rightEnd_.edgeListRightNeighbor = null;
        hash_[0] = leftEnd_;
        hash_[hashSize_ - 1] = rightEnd_;
    }

    public void Dispose() {
        HalfEdge halfEdge = leftEnd_;

        while (halfEdge != rightEnd_) {
            HalfEdge previousHalfEdge = halfEdge;
            halfEdge = halfEdge.edgeListRightNeighbor;
            previousHalfEdge.Dispose();
        }

        leftEnd_ = null;
        rightEnd_.Dispose();
        rightEnd_ = null;

        for (int i = 0; i < hashSize_; ++i) {
            hash_[i] = null;
        }

        hash_ = null;
    }

    public void Insert(HalfEdge lb, HalfEdge newHalfEdge) {
        newHalfEdge.edgeListLeftNeighbor = lb;
        newHalfEdge.edgeListRightNeighbor = lb.edgeListRightNeighbor;
        
        lb.edgeListRightNeighbor.edgeListLeftNeighbor = newHalfEdge;
        lb.edgeListRightNeighbor = newHalfEdge;
    }

    public void Remove(HalfEdge halfEdge) {
        halfEdge.edgeListLeftNeighbor.edgeListRightNeighbor = halfEdge.edgeListRightNeighbor;
        halfEdge.edgeListRightNeighbor.edgeListLeftNeighbor = halfEdge.edgeListLeftNeighbor;

        halfEdge.edge = Edge.DELETED;
        halfEdge.edgeListLeftNeighbor = halfEdge.edgeListRightNeighbor = null;
    }

    public HalfEdge EdgeListLeftNeighbor(Vector3 pos) {
        int bucket = (int)((pos.x - xMin_) / deltaX_ * hashSize_);
        if (bucket < 0) {
            bucket = 0;
        }
        if (bucket >= hashSize_) {
            bucket = hashSize_ - 1;
        }
        HalfEdge halfEdge = GetHash (bucket);
        if (halfEdge == null) {
            int i = 1;
            while (true) {
                if ((halfEdge = GetHash (bucket - i)) != null)
                    break;
                if ((halfEdge = GetHash (bucket + i)) != null)
                    break;
                i++;
            }
        }
        
        if (halfEdge == leftEnd_ || (halfEdge != rightEnd_ && halfEdge.IsLeftOf (pos))) {
            do {
                halfEdge = halfEdge.edgeListRightNeighbor;
            } while (halfEdge != rightEnd_ && halfEdge.IsLeftOf(pos));
            halfEdge = halfEdge.edgeListLeftNeighbor;
        } else {
            do {
                halfEdge = halfEdge.edgeListLeftNeighbor;
            } while (halfEdge != leftEnd_ && !halfEdge.IsLeftOf(pos));
        }
		
        if (bucket > 0 && bucket < hashSize_ - 1) {
            hash_[bucket] = halfEdge;
        }
        return halfEdge;
    }

    HalfEdge GetHash(int bucket) {
        if (bucket < 0 || bucket >= hashSize_) {
            return null;
        }

        HalfEdge halfEdge = hash_[bucket];
        if (halfEdge == null || halfEdge.edge != Edge.DELETED) return halfEdge;
        
        hash_[bucket] = null;
        return null;

    }
}
}