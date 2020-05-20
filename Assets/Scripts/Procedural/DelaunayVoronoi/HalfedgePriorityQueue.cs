using UnityEngine;

namespace Procedural {
sealed class HalfedgePriorityQueue {
    Halfedge[] hash_;
    int count_;
    int minBucket_;
    int hashSize_;

    float zMin_;
    float deltaZ_;

    public HalfedgePriorityQueue(float zMin, float deltaZ, int sqrtNSites) {
        zMin_ = zMin;
        deltaZ_ = deltaZ;
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
        hash_ = new Halfedge[hashSize_];

        for (int i = 0; i < hashSize_; i++) {
            hash_[i] = Halfedge.CreateDummy();
            hash_[i].nextInPriorityQueue = null;
        }
    }

    public void Insert(Halfedge halfedge) {
        Halfedge previous, next;

        int insertionBucket = Bucket(halfedge);

        if (insertionBucket < minBucket_) {
            minBucket_ = insertionBucket;
        }

        previous = hash_[insertionBucket];

        while ((next = previous.nextInPriorityQueue) != null &&
               (halfedge.zStar > next.zStar || (halfedge.zStar == next.zStar && halfedge.vertex.X > next.vertex.X))) {
            previous = next;
        }

        halfedge.nextInPriorityQueue = previous.nextInPriorityQueue;
        previous.nextInPriorityQueue = halfedge;
        ++count_;
    }

    public void Remove(Halfedge halfedge) {
        Halfedge previous;
        int removalBucket = Bucket(halfedge);

        if (halfedge.vertex != null) {
            previous = hash_[removalBucket];

            while (previous.nextInPriorityQueue != halfedge) {
                previous = previous.nextInPriorityQueue;
            }

            previous.nextInPriorityQueue = halfedge.nextInPriorityQueue;
            count_--;
            halfedge.vertex = null;
            halfedge.nextInPriorityQueue = null;
            halfedge.Dispose();
        }
    }

    int Bucket(Halfedge halfedge) {
        int bucket = (int) ((halfedge.zStar - zMin_) / deltaZ_ * hashSize_);

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

    public Vector3 Min() {
        AdjustMinBucket();
        Halfedge result = hash_[minBucket_].nextInPriorityQueue;
        return new Vector3(result.vertex.X, result.zStar);
    }

    public Halfedge ExtractMin() {
        Halfedge result = hash_[minBucket_].nextInPriorityQueue;

        hash_[minBucket_].nextInPriorityQueue = result.nextInPriorityQueue;
        count_--;
        result.nextInPriorityQueue = null;

        return result;
    }
}
}