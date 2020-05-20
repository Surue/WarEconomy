using UnityEngine;

namespace Procedural {
internal sealed class EdgeList {
    float deltaX_;
    float xMin_;

    int hashSize_;
    Halfedge[] hash_;
    Halfedge leftEnd_;
    public Halfedge LeftEnd => leftEnd_;

    Halfedge rightEnd_;

    public Halfedge RightEnd => rightEnd_;
    
    public EdgeList(float xMin, float deltaX, int sqrtNSite) {
        xMin_ = xMin;
        deltaX_ = deltaX;
        hashSize_ = 2 * sqrtNSite;
        
        hash_ = new Halfedge[hashSize_];
        
        leftEnd_ = Halfedge.CreateDummy ();
        rightEnd_ = Halfedge.CreateDummy ();
        
        leftEnd_.edgeListLeftNeighbor = null;
        leftEnd_.edgeListRightNeighbor = rightEnd_;
        rightEnd_.edgeListLeftNeighbor = leftEnd_;
        rightEnd_.edgeListRightNeighbor = null;
        hash_[0] = leftEnd_;
        hash_[hashSize_ - 1] = rightEnd_;
    }

    public void Dispose() {
        Halfedge halfedge = leftEnd_;
        Halfedge prevHalfedge;

        while (halfedge != rightEnd_) {
            prevHalfedge = halfedge;
            halfedge = halfedge.edgeListRightNeighbor;
            prevHalfedge.Dispose();
        }

        leftEnd_ = null;
        rightEnd_.Dispose();
        rightEnd_ = null;

        for (int i = 0; i < hashSize_; ++i) {
            hash_[i] = null;
        }

        hash_ = null;
    }

    public void Insert(Halfedge lb, Halfedge newHalfedge) {
        newHalfedge.edgeListLeftNeighbor = lb;
        newHalfedge.edgeListRightNeighbor = lb.edgeListRightNeighbor;
        
        lb.edgeListRightNeighbor.edgeListLeftNeighbor = newHalfedge;
        lb.edgeListRightNeighbor = newHalfedge;
    }

    public void Remove(Halfedge halfedge) {
        halfedge.edgeListLeftNeighbor.edgeListRightNeighbor = halfedge.edgeListRightNeighbor;
        halfedge.edgeListRightNeighbor.edgeListLeftNeighbor = halfedge.edgeListLeftNeighbor;

        halfedge.edge = Edge.DELETED;
        halfedge.edgeListLeftNeighbor = halfedge.edgeListRightNeighbor = null;
    }

    public Halfedge EdgeListLeftNeighbor(Vector3 pos) {
        int i, bucket;
        Halfedge halfEdge;
		
        /* Use hash table to get close to desired halfedge */
        bucket = (int)((pos.x - xMin_) / deltaX_ * hashSize_);
        if (bucket < 0) {
            bucket = 0;
        }
        if (bucket >= hashSize_) {
            bucket = hashSize_ - 1;
        }
        halfEdge = GetHash (bucket);
        if (halfEdge == null) {
            for (i = 1; true; ++i) {
                if ((halfEdge = GetHash (bucket - i)) != null)
                    break;
                if ((halfEdge = GetHash (bucket + i)) != null)
                    break;
            }
        }
        /* Now search linear list of halfedges for the correct one */
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
		
        /* Update hash table and reference counts */
        if (bucket > 0 && bucket < hashSize_ - 1) {
            hash_[bucket] = halfEdge;
        }
        return halfEdge;
    }

    Halfedge GetHash(int bucket) {
        Halfedge halfedge;

        if (bucket < 0 || bucket >= hashSize_) {
            return null;
        }

        halfedge = hash_[bucket];
        if (halfedge != null && halfedge.edge == Edge.DELETED) {
            hash_[bucket] = null;
            return null;
        } else {
            return halfedge;
        }
    }
}
}