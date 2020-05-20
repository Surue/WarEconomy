using UnityEngine;

namespace Procedural {
sealed class HalfEdgePriorityQueue {
    HalfEdge[] hash_;
    int count_;
    int minBucket_;
    int hashSize_;

    float yMin_;
    float deltaY_;

    public HalfEdgePriorityQueue(float yMin, float deltaY, int sqrtNSites) {
        yMin_ = yMin;
        deltaY_ = deltaY;
        hashSize_ = 4 * sqrtNSites;
        Init();
    }

    public void Dispose() {
        for (int i = 0; i < hashSize_; ++i) {
            hash_[i].Dispose();
            hash_[i] = null;
        }

        hash_ = null;
    }

    void Init() {
        count_ = 0;
        minBucket_ = 0;
        hash_ = new HalfEdge[hashSize_];

        for (int i = 0; i < hashSize_; i++) {
            hash_[i] = HalfEdge.CreateDummy();
            hash_[i].nextInPriorityQueue = null;
        }
    }

    public void Insert(HalfEdge halfEdge) {
        HalfEdge next;
        int insertionBucket = Bucket (halfEdge);
        if (insertionBucket < minBucket_) {
            minBucket_ = insertionBucket;
        }
        
        HalfEdge previous = hash_[insertionBucket];
        while ((next = previous.nextInPriorityQueue) != null && 
               (halfEdge.yStar  > next.yStar || halfEdge.yStar == next.yStar && halfEdge.vertex.X > next.vertex.X)) {
            previous = next;
        }
        
        halfEdge.nextInPriorityQueue = previous.nextInPriorityQueue; 
        previous.nextInPriorityQueue = halfEdge;
        ++count_;
    }

    public void Remove(HalfEdge halfEdge) {
        int removalBucket = Bucket (halfEdge);

        if (halfEdge.vertex == null) return;
        
        HalfEdge previous = hash_[removalBucket];
        while (previous.nextInPriorityQueue != halfEdge) {
            previous = previous.nextInPriorityQueue;
        }
        previous.nextInPriorityQueue = halfEdge.nextInPriorityQueue;
        count_--;
        halfEdge.vertex = null;
        halfEdge.nextInPriorityQueue = null;
        halfEdge.Dispose ();
    }

    int Bucket(HalfEdge halfEdge) {
        int bucket = (int) ((halfEdge.yStar - yMin_) / deltaY_ * hashSize_);

        if (bucket < 0) {
            bucket = 0;
        }

        if (bucket >= hashSize_) {
            bucket = hashSize_ - 1;
        }

        return bucket;
    }

    bool IsEmpty(int bucket) {
        return (hash_[bucket].nextInPriorityQueue == null);
    }

    void AdjustMinBucket() {
        while (minBucket_ < hashSize_ - 1 && IsEmpty(minBucket_)) {
            ++minBucket_;
        }
    }

    public bool Empty() {
        return count_ == 0;
    }

    public Vector2 Min() {
        AdjustMinBucket();
        HalfEdge result = hash_[minBucket_].nextInPriorityQueue;
        return new Vector2(result.vertex.X, result.yStar);
    }

    public HalfEdge ExtractMin() {
        HalfEdge result = hash_[minBucket_].nextInPriorityQueue;

        hash_[minBucket_].nextInPriorityQueue = result.nextInPriorityQueue;
        count_--;
        result.nextInPriorityQueue = null;

        return result;
    }
}
}