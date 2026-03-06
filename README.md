## Dataset Limitations

The dataset consists of approximately 230 original images with additional augmentations applied, resulting in around 920 training samples.

Most images contain at least one labeled object, which introduces a bias in the dataset. The model therefore learns that an object is almost always present in the image.

Because the dataset contains very few hard negative examples (images without labeled objects), the model tends to produce high confidence predictions even when the object is not present.

This results in false positives where unrelated objects may still be classified with high confidence scores.

Improving the dataset with more hard negatives and greater scene variation would likely improve the model's ability to generalize.
