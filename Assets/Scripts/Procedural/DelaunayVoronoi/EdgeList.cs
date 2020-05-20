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
        
        leftEnd_ = Halfedge.CreateDummy();
        rightEnd_ = Halfedge.CreateDummy();

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

    public void Insert(Halfedge left, Halfedge newHalfedge) {
        newHalfedge.edgeListLeftNeighbor = left;
        newHalfedge.edgeListRightNeighbor = left.edgeListRightNeighbor;
        left.edgeListRightNeighbor.edgeListLeftNeighbor = newHalfedge;
        left.edgeListRightNeighbor = newHalfedge;
    }

    public void Remove(Halfedge halfedge) {
        halfedge.edgeListLeftNeighbor.edgeListRightNeighbor = halfedge.edgeListRightNeighbor;
        halfedge.edgeListRightNeighbor.edgeListLeftNeighbor = halfedge.edgeListLeftNeighbor;

        halfedge.edge = Edge.DELETED;
        halfedge.edgeListLeftNeighbor = halfedge.edgeListRightNeighbor = null;
    }

    public Halfedge EdgeListLeftNeighbor(Vector3 pos) {
        Halfedge halfedge;

        int bucket = (int) ((pos.x - xMin_) / deltaX_ * hashSize_);
        if (bucket < 0) {
            bucket = 0;
        }

        if (bucket > hashSize_) {
            bucket = hashSize_ - 1;
        }

        halfedge = GetHash(bucket);

        if (halfedge == null) {
            for (int i = 1; true; i++) {
                if ((halfedge = GetHash(bucket - i)) != null) {
                    break;
                }

                if ((halfedge = GetHash(bucket + i)) != null) {
                    break;
                }
            }
        }

        if (halfedge == leftEnd_ || (halfedge != rightEnd_ && halfedge.IsLeftOf(pos))) {
            do {
                halfedge = halfedge.edgeListRightNeighbor;
            } while (halfedge != rightEnd_ && halfedge.IsLeftOf(pos));

            halfedge = halfedge.edgeListLeftNeighbor;
        } else {
            do {
                halfedge = halfedge.edgeListLeftNeighbor;
            } while (halfedge != leftEnd_ && !halfedge.IsLeftOf(pos));
        }

        if (bucket > 0 && bucket < hashSize_ - 1) {
            hash_[bucket] = halfedge;
        }

        return halfedge;
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