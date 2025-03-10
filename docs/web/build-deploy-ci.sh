# Build site with correct base URL and version
SHA=($(git rev-parse --short HEAD))
export VERSION=$(jq .version package.json -r)
VERSION=$VERSION+$SHA
export BASE_URL=/ide/v/$VERSION
npm run build

# Deploy to S3
BUCKET_NAME=$(aws ssm get-parameter --name "/$CONTEXTIVE_STAGE/docs/bucketName" --query Parameter.Value --output text)
S3_URL=s3://$BUCKET_NAME$BASE_URL
echo Deploying to $S3_URL
aws s3 sync dist $S3_URL

# Invalidate Cache
if [[ -z "$CI" ]]; then
    GITHUB_JOB=`date +%s`
fi
DISTRIBUTION_ID=$(aws ssm get-parameter --name "/$CONTEXTIVE_STAGE/docs/distributionId" --query Parameter.Value --output text)
aws cloudfront create-invalidation \
    --distribution-id $DISTRIBUTION_ID \
    --invalidation-batch "Paths={Quantity=1,Items=[$BASE_URL/*]},CallerReference=$GITHUB_JOB"

# Output outcome
if [[ "$CONTEXTIVE_STAGE" = "prod" ]]; then
    echo Site live at https://docs.contextive.tech$BASE_URL
else
    echo Site live for preview at https://docs.test.contextive.tech$BASE_URL
fi