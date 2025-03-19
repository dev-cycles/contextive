# Build site with correct base URL and version
export CONTEXTIVE_VERSION=$(jq .version package.json -r)
if [[ "$CONTEXTIVE_STAGE" != "prod" ]]; then
    export CONTEXTIVE_SHA=$(git rev-parse --short HEAD)
    CONTEXTIVE_VERSION=$CONTEXTIVE_SHA
fi
export BASE_URL=/ide

if [[ "$CONTEXTIVE_ARCHIVE" ]]; then
    BASE_URL=$BASE_URL/v/$CONTEXTIVE_VERSION
fi

if [[ "$CONTEXTIVE_STAGE" == "prod" ]]; then    
    CONTEXTIVE_VERSION=v$CONTEXTIVE_VERSION
fi
rm -rf dist
npm run build

# Deploy to S3
BUCKET_NAME=$(aws ssm get-parameter --name "/$CONTEXTIVE_STAGE/docs/bucketName" --query Parameter.Value --output text)
S3_URL=s3://$BUCKET_NAME$BASE_URL
echo Deploying to $S3_URL
aws s3 sync dist $S3_URL

# Invalidate Cache
CALLER_REF=`date +%s`
DISTRIBUTION_ID=$(aws ssm get-parameter --name "/$CONTEXTIVE_STAGE/docs/distributionId" --query Parameter.Value --output text)
aws cloudfront create-invalidation \
    --distribution-id $DISTRIBUTION_ID \
    --invalidation-batch "Paths={Quantity=1,Items=[$BASE_URL/*]},CallerReference=$CALLER_REF"

# Output outcome
if [[ "$CONTEXTIVE_STAGE" = "prod" ]]; then
    echo ::notice ::Site live at https://docs.contextive.tech$BASE_URL
else
    echo ::notice ::Site live for preview at https://docs.test.contextive.tech$BASE_URL
fi